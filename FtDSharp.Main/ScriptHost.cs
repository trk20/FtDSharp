using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BrilliantSkies.Profiling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using UnityEngine;

namespace FtDSharp
{
    public sealed class ScriptHost
    {
        private object? _instance;
        private Action? _onPhysicsTick;
        private Action? _onStop;
        private IProviderScope? _scope;
        private static readonly MetadataReference[] _defaultReferences = BuildDefaultReferences();
        private const string _scriptPrelude =
            "global using System;\n" +
            "global using System.Collections.Generic;\n" +
            "global using System.Linq;\n" +
            "global using UnityEngine;\n" +
            "global using FtDSharp;\n" +
            "global using static FtDSharp.Logging;\n" +
            "#line 1\n";

        private static readonly ImmutableArray<DiagnosticAnalyzer> _bannedApiAnalyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new CSharpSymbolIsBannedAnalyzer());

        private static readonly AnalyzerOptions _bannedApiAnalyzerOptions = new(
            ImmutableArray.Create<AdditionalText>(new InMemoryAdditionalText("BannedSymbols.txt",
                // File system & network
                "N:System.IO;File system access is not allowed.\n" +
                "N:System.Net;Direct network access is not allowed.\n" +
                // Reflection & type loading
                "N:System.Reflection;Reflection is forbidden to prevent sandbox escapes.\n" +
                "M:System.Type.GetType(System.String);Type loading by name is not allowed.\n" +
                "M:System.Type.GetType(System.String,System.Boolean);Type loading by name is not allowed.\n" +
                "M:System.Type.GetType(System.String,System.Boolean,System.Boolean);Type loading by name is not allowed.\n" +
                "M:System.Type.GetType(System.String,System.Func{System.Reflection.AssemblyName,System.Reflection.Assembly},System.Func{System.Reflection.Assembly,System.String,System.Boolean,System.Type});Type loading by name is not allowed.\n" +
                "M:System.Type.GetType(System.String,System.Func{System.Reflection.AssemblyName,System.Reflection.Assembly},System.Func{System.Reflection.Assembly,System.String,System.Boolean,System.Type},System.Boolean);Type loading by name is not allowed.\n" +
                "M:System.Type.GetType(System.String,System.Func{System.Reflection.AssemblyName,System.Reflection.Assembly},System.Func{System.Reflection.Assembly,System.String,System.Boolean,System.Type},System.Boolean,System.Boolean);Type loading by name is not allowed.\n" +
                "T:System.Activator;Dynamic object creation via Activator is not allowed.\n" +
                // Low-level / unsafe memory
                "N:System.Runtime.InteropServices;Native interop is not allowed.\n" +
                "T:System.Runtime.CompilerServices.Unsafe;Unsafe memory operations are not allowed.\n" +
                // Process & runtime manipulation
                "N:System.Diagnostics;Process execution is not allowed.\n" +
                "N:System.Runtime.Loader;Assembly loading manipulation is not allowed.\n" +
                "T:System.AppDomain;Application domain manipulation is not allowed.\n" +
                "T:System.Environment;Environment variable access is not allowed.\n" +
                "T:System.Threading.Thread;Direct thread creation is not allowed.\n" +
                "T:System.Threading.Tasks.Parallel;Arbitrary parallelism is not allowed.\n" +
                // Meta-compilation & dynamic code gen
                "N:Microsoft.CodeAnalysis;Meta-compilation is not allowed.\n" +
                "M:System.Linq.Expressions.Expression`1.Compile;Dynamic compilation is not allowed.\n" +
                // Dynamic keyword support
                "N:System.Dynamic;Dynamic dispatch is not allowed.\n" +
                "N:Microsoft.CSharp;Dynamic keyword support types are not allowed.\n" +
                // Dangerous properties
                "P:System.Exception.TargetSite;TargetSite exposes reflection and is not allowed.\n")));

        public bool Active => _instance != null;
        public string? CurrentHash { get; private set; }
        public TimeSpan LastCompileTime { get; private set; }
        public string? LastError { get; private set; }

        internal (bool Success, Diagnostic[] Diagnostics) Compile(string code, string hash)
        {
            LastError = null;
            var diagnosticsList = new List<Diagnostic>();

            if (ScriptCompilationCache.TryGet(hash, out _))
            {
                return (true, diagnosticsList.ToArray());
            }

            try
            {
                var sw = Stopwatch.StartNew();

                SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(_scriptPrelude + code);

                var compilation = CSharpCompilation.Create(
                    assemblyName: $"FtDSharpScript_{Guid.NewGuid():N}",
                    syntaxTrees: new[] { syntaxTree },
                    references: _defaultReferences,
                    options: new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        optimizationLevel: OptimizationLevel.Release,
                        allowUnsafe: false));

                if (ContainsDynamicKeyword(syntaxTree))
                {
                    LastError = "Banned API usage:\nDynamic dispatch is not allowed.";
                    return (false, diagnosticsList.ToArray());
                }

                ImmutableArray<Diagnostic> validationDiags = RunBannedApiAnalysis(compilation);
                if (validationDiags.Length > 0)
                {
                    LastError = "Banned API usage:\n" + string.Join("\n",
                        validationDiags.Select(static d => d.GetMessage()));
                    diagnosticsList.AddRange(validationDiags);
                    return (false, diagnosticsList.ToArray());
                }

                using var ms = new MemoryStream();
                EmitResult emitResult = compilation.Emit(ms);
                if (!emitResult.Success)
                {
                    diagnosticsList.AddRange(emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
                    LastError = string.Join("\n", diagnosticsList.Select(d => d.ToString()));
                    return (false, diagnosticsList.ToArray());
                }

                var assembly = Assembly.Load(ms.ToArray());
                ScriptCompilationCache.Store(hash, assembly);

                LastCompileTime = sw.Elapsed;
                return (true, diagnosticsList.ToArray());
            }
            catch (Exception ex)
            {
                LastError = $"Error compiling script: {ex.Message}";
                return (false, diagnosticsList.ToArray());
            }
        }

        internal bool Instantiate(string hash, IProviderScope? scope)
        {
            LastError = null;

            if (!ScriptCompilationCache.TryGet(hash, out Assembly? assembly))
            {
                LastError = "No compiled assembly found for the given hash. Compile first.";
                return false;
            }

            try
            {
                Type type = assembly!
                    .GetTypes()
                    .FirstOrDefault(HasAttributedEntryPointMethods);

                if (type == null)
                {
                    LastError = "No public class with entry point methods found. Your script must declare at least one method with [OnPhysicsTick], [OnStart], or [OnStop].";
                    return false;
                }

                var validationError = ValidateEntryPointMethods(type);
                if (validationError != null)
                {
                    LastError = validationError;
                    return false;
                }

                MethodInfo? tickMethod = FindEntryPointMethod<OnPhysicsTickAttribute>(type);
                MethodInfo? startMethod = FindEntryPointMethod<OnStartAttribute>(type);
                MethodInfo? stopMethod = FindEntryPointMethod<OnStopAttribute>(type);

                using (scope != null ? ScriptContext.Push(scope) : null)
                {
                    _instance = Activator.CreateInstance(type)!;
                    _onPhysicsTick = CreateEntryPointDelegate(_instance, tickMethod);
                    _onStop = CreateEntryPointDelegate(_instance, stopMethod);
                    CreateEntryPointDelegate(_instance, startMethod)?.Invoke();
                }

                CurrentHash = hash;
                _scope = scope;
                return true;
            }
            catch (Exception ex)
            {
                LastError = $"Error instantiating script: {ex.GetBaseException().Message}";
                _instance = null;
                _onPhysicsTick = null;
                _onStop = null;
                _scope = null;
                CurrentHash = null;
                return false;
            }
        }

        internal void Tick(IProviderScope scope)
        {
            if (_instance == null) return;

            using IDisposable currentScope = ScriptContext.Push(scope);

            BrilliantSkies.Profiling.ProfileTypes.IProfile? profile = AbstractModule<FtDSharpProfiler>.Instance?.ScriptExecution;
            var startTime = profile?.Start() ?? 0;
            try
            {
                _onPhysicsTick?.Invoke();
            }
            catch (Exception ex)
            {
                scope.Log.Error($"Error during script execution: {ex.Message}\n{ex.StackTrace}");
                Deactivate();
            }
            finally
            {
                profile?.Finish(startTime);
            }
        }

        public void Deactivate()
        {
            if (_instance == null) return;

            try
            {
                if (_onStop != null && _scope != null)
                {
                    using IDisposable ctx = ScriptContext.Push(_scope);
                    _onStop.Invoke();
                }
            }
            catch { /* swallow — script is being torn down */ }
            finally
            {
                if (_instance is IDisposable disposable)
                {
                    try { disposable.Dispose(); } catch { }
                }

                if (_scope is IDisposable disposableScope)
                {
                    disposableScope.Dispose();
                }

                _instance = null;
                _onPhysicsTick = null;
                _onStop = null;
                _scope = null;
                CurrentHash = null;
            }
        }

        internal static string ComputeHash(string input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }

        private static ImmutableArray<Diagnostic> RunBannedApiAnalysis(CSharpCompilation compilation)
        {
            var cwaOptions = new CompilationWithAnalyzersOptions(
                _bannedApiAnalyzerOptions,
                onAnalyzerException: null,
                concurrentAnalysis: true,
                logAnalyzerExecutionTime: false,
                reportSuppressedDiagnostics: false);

            var cwa = new CompilationWithAnalyzers(compilation, _bannedApiAnalyzers, cwaOptions);
            ImmutableArray<Diagnostic> diagnostics = cwa.GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
            return diagnostics.Where(static d => d.Id is "RS0030" or "RS0031").ToImmutableArray();
        }

        private static bool ContainsDynamicKeyword(SyntaxTree syntaxTree)
        {
            return syntaxTree.GetRoot()
                .DescendantTokens()
                .Any(static token => string.Equals(token.Text, "dynamic", StringComparison.Ordinal));
        }

        private static MetadataReference[] BuildDefaultReferences()
        {
            static void AddReference(List<MetadataReference> refs, HashSet<string> seen, string? location)
            {
                if (string.IsNullOrWhiteSpace(location))
                {
                    return;
                }

                if (!seen.Add(location))
                {
                    return;
                }

                try
                {
                    refs.Add(MetadataReference.CreateFromFile(location));
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"[FtDSharp] Failed to add reference '{location}': {ex.Message}");
                }
            }

            Assembly[] candidates =
            {
                typeof(object).Assembly,
                typeof(Enumerable).Assembly,
                typeof(Console).Assembly,
                typeof(Uri).Assembly,
                typeof(OnPhysicsTickAttribute).Assembly,
                typeof(ScriptHost).Assembly,
                typeof(Vector3).Assembly
            };

            var refs = new List<MetadataReference>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string trustedPlatformAssemblies)
            {
                foreach (var reference in trustedPlatformAssemblies.Split(Path.PathSeparator))
                {
                    AddReference(refs, seen, reference);
                }
            }

            foreach (Assembly asm in candidates)
            {
                AddReference(refs, seen, asm.Location);
            }

            var coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location) ?? string.Empty;
            string[] fallback =
            {
                Path.Combine(coreDir, "mscorlib.dll"),
                Path.Combine(coreDir, "System.dll"),
                Path.Combine(coreDir, "System.Core.dll"),
                Path.Combine(coreDir, "System.Xml.dll"),
                Path.Combine(coreDir, "netstandard.dll")
            };

            foreach (var reference in fallback)
            {
                if (File.Exists(reference))
                {
                    AddReference(refs, seen, reference);
                }
            }

            return refs.ToArray();
        }

        private static bool HasAttributedEntryPointMethods(Type type) => type.IsClass && type.IsPublic && GetAttributedEntryPointMethods(type).Length > 0;

        private static MethodInfo? FindEntryPointMethod<TAttribute>(Type type)
            where TAttribute : Attribute
        {
            return type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(method => method.GetCustomAttribute<TAttribute>() != null && IsValidEntryPointMethod(method));
        }

        private static string? ValidateEntryPointMethods(Type type)
        {
            foreach (MethodInfo method in GetAttributedEntryPointMethods(type))
            {
                if (IsValidEntryPointMethod(method))
                {
                    continue;
                }

                return $"Entry point method '{type.FullName}.{method.Name}' must be a public instance parameterless void method.";
            }

            return null;
        }

        private static MethodInfo[] GetAttributedEntryPointMethods(Type type)
        {
            return type
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(HasEntryPointAttribute)
                .ToArray();
        }

        private static bool IsValidEntryPointMethod(MethodInfo method)
        {
            return method.IsPublic
                && !method.IsStatic
                && method.ReturnType == typeof(void)
                && method.GetParameters().Length == 0;
        }

        private static bool HasEntryPointAttribute(MethodInfo method)
        {
            return method.GetCustomAttribute<OnPhysicsTickAttribute>() != null
                || method.GetCustomAttribute<OnStartAttribute>() != null
                || method.GetCustomAttribute<OnStopAttribute>() != null;
        }

        private static Action? CreateEntryPointDelegate(object instance, MethodInfo? method)
        {
            return method == null ? null
                : (Action)Delegate.CreateDelegate(typeof(Action), instance, method);
        }
    }
}
