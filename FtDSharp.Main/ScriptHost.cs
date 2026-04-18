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
        private IFtDSharp? _instance;
        private string? _hash;
        private string? _lastError;
        private IScriptContext? _context;
        private static readonly MetadataReference[] DefaultReferences = BuildDefaultReferences();
        private const string ScriptPrelude = "#line 1\n";

        private static readonly ImmutableArray<DiagnosticAnalyzer> BannedApiAnalyzers =
            ImmutableArray.Create<DiagnosticAnalyzer>(new CSharpSymbolIsBannedAnalyzer());

        private static readonly AnalyzerOptions BannedApiAnalyzerOptions = new(
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
        public string? CurrentHash => _hash;
        public TimeSpan LastCompileTime { get; private set; }
        public string? LastError => _lastError;

        internal (bool Success, Diagnostic[] Diagnostics) Compile(string code, string hash)
        {
            _lastError = null;
            var diagnosticsList = new List<Diagnostic>();

            if (ScriptCompilationCache.TryGet(hash, out _))
            {
                return (true, diagnosticsList.ToArray());
            }

            try
            {
                var sw = Stopwatch.StartNew();

                var syntaxTree = CSharpSyntaxTree.ParseText(ScriptPrelude + code);

                var compilation = CSharpCompilation.Create(
                    assemblyName: $"FtDSharpScript_{Guid.NewGuid():N}",
                    syntaxTrees: new[] { syntaxTree },
                    references: DefaultReferences,
                    options: new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        optimizationLevel: OptimizationLevel.Release,
                        allowUnsafe: false,
                        usings: new[]
                        {
                            "System",
                            "System.Collections.Generic",
                            "System.Linq",
                            "UnityEngine",
                            "FtDSharp",
                        }));

                var validationDiags = RunBannedApiAnalysis(compilation);
                if (validationDiags.Length > 0)
                {
                    _lastError = "Banned API usage:\n" + string.Join("\n",
                        validationDiags.Select(static d => d.GetMessage()));
                    diagnosticsList.AddRange(validationDiags);
                    return (false, diagnosticsList.ToArray());
                }

                using var ms = new MemoryStream();
                EmitResult emitResult = compilation.Emit(ms);
                if (!emitResult.Success)
                {
                    diagnosticsList.AddRange(emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
                    _lastError = string.Join("\n", diagnosticsList.Select(d => d.ToString()));
                    return (false, diagnosticsList.ToArray());
                }

                var assembly = Assembly.Load(ms.ToArray());
                ScriptCompilationCache.Store(hash, assembly);

                LastCompileTime = sw.Elapsed;
                return (true, diagnosticsList.ToArray());
            }
            catch (Exception ex)
            {
                _lastError = $"Error compiling script: {ex.Message}";
                return (false, diagnosticsList.ToArray());
            }
        }

        internal bool Instantiate(string hash, IScriptContext? ctx)
        {
            _lastError = null;

            if (!ScriptCompilationCache.TryGet(hash, out var assembly))
            {
                _lastError = "No compiled assembly found for the given hash. Compile first.";
                return false;
            }

            try
            {
                var type = assembly!.GetTypes().FirstOrDefault(t => typeof(IFtDSharp).IsAssignableFrom(t));
                if (type == null)
                {
                    _lastError = "No class implementing IFtDSharp found. Your script must have a class that implements IFtDSharp.";
                    return false;
                }

                using (ctx != null ? ScriptApi.PushContext(ctx) : null)
                {
                    _instance = (IFtDSharp)Activator.CreateInstance(type)!;
                }

                _hash = hash;
                _context = ctx;
                return true;
            }
            catch (Exception ex)
            {
                _lastError = $"Error instantiating script: {ex.Message}";
                _instance = null;
                _hash = null;
                return false;
            }
        }

        internal void Tick(IScriptContext ctx)
        {
            if (_instance == null) return;

            _context = ctx;

            using var scope = ScriptApi.PushContext(ctx);

            var profile = AbstractModule<FtDSharpProfiler>.Instance.ScriptExecution;
            var startTime = profile.Start();
            try
            {
                _instance.Update();
            }
            catch (Exception ex)
            {
                ctx.Log.Error($"Error during script execution: {ex.Message}\n{ex.StackTrace}");
                Deactivate();
            }
            finally
            {
                profile.Finish(startTime);
            }
        }

        public void Deactivate()
        {
            // Dispose of anything that needs disposing
            if (_instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _instance?.OnStop();
            _instance = null;
            _hash = null;
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
                BannedApiAnalyzerOptions,
                onAnalyzerException: null,
                concurrentAnalysis: true,
                logAnalyzerExecutionTime: false,
                reportSuppressedDiagnostics: false);

            var cwa = new CompilationWithAnalyzers(compilation, BannedApiAnalyzers, cwaOptions);
            var diagnostics = cwa.GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
            return diagnostics.Where(static d => d.Id is "RS0030" or "RS0031").ToImmutableArray();
        }

        private static MetadataReference[] BuildDefaultReferences()
        {
            Assembly[] candidates =
            {
                typeof(object).Assembly,
                typeof(Enumerable).Assembly,
                typeof(Console).Assembly,
                typeof(Uri).Assembly,
                typeof(IFtDSharp).Assembly,
                typeof(Vector3).Assembly,
                typeof(ScriptHost).Assembly
            };

            var refs = new List<MetadataReference>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var asm in candidates)
            {
                var location = asm.Location;
                if (string.IsNullOrEmpty(location) && asm == typeof(ScriptHost).Assembly)
                {
                    var fallbackPath = Path.Combine(ModInfo.ModPath, "FtDSharp.dll");
                    if (File.Exists(fallbackPath))
                    {
                        location = fallbackPath;
                    }
                }
                if (string.IsNullOrEmpty(location)) continue;
                if (!seen.Add(location)) continue;
                try
                {
                    refs.Add(MetadataReference.CreateFromFile(location));
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"[FtDSharp] Failed to add reference '{asm.FullName}': {ex.Message}");
                }
            }

            var coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location) ?? string.Empty;
            string[] fallback =
            {
                Path.Combine(coreDir, "mscorlib.dll"),
                Path.Combine(coreDir, "System.dll"),
                Path.Combine(coreDir, "System.Core.dll"),
                Path.Combine(coreDir, "System.Xml.dll"),
                Path.Combine(coreDir, "netstandard.dll"),
                Path.Combine(coreDir, "FtDSharp.dll")
            };

            foreach (var reference in fallback)
            {
                if (!File.Exists(reference)) continue;
                if (!seen.Add(reference)) continue;
                try
                {
                    refs.Add(MetadataReference.CreateFromFile(reference));
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"[FtDSharp] Failed to add fallback reference '{reference}': {ex.Message}");
                }
            }

            return refs.ToArray();
        }
    }
}
