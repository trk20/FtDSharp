using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BrilliantSkies.Profiling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using UnityEngine;

namespace FtDSharp
{
    public class ScriptHost
    {
        private IFtDSharp? _instance;
        private Assembly? _assembly;
        private DateTime _startUtc;
        private string? _hash;
        private string? _lastError;
        private static readonly MetadataReference[] DefaultReferences = BuildDefaultReferences();
        private const string ScriptPrelude = "#line 1\n";

        public bool Active => _instance != null;
        public string? CurrentHash => _hash;
        public TimeSpan LastCompileTime { get; private set; }
        public string? LastError => _lastError;

        internal (bool Success, Diagnostic[] Diagnostics) LoadScript(string code, string hash, IScriptContext? ctx = null)
        {
            _lastError = null;
            var diagnosticsList = new List<Diagnostic>();

            try
            {
                var sw = Stopwatch.StartNew();
                byte[] ilBytes;

                if (ScriptCompilationCache.TryGet(hash, out var cached))
                {
                    ilBytes = cached!;
                }
                else
                {
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

                    var validationErrors = ForbiddenNamespaceValidator.GetInvalidUsages(compilation, syntaxTree);
                    if (validationErrors.Count > 0)
                    {
                        _lastError = "Forbidden namespace usage:\n" + string.Join("\n", validationErrors);
                        _instance = null;
                        _assembly = null;
                        _hash = null;
                        return (false, diagnosticsList.ToArray());
                    }

                    using var ms = new MemoryStream();
                    EmitResult emitResult = compilation.Emit(ms);
                    if (!emitResult.Success)
                    {
                        diagnosticsList.AddRange(emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
                        _lastError = string.Join("\n", diagnosticsList.Select(d => d.ToString()));
                        _instance = null;
                        _assembly = null;
                        _hash = null;
                        return (false, diagnosticsList.ToArray());
                    }

                    ilBytes = ms.ToArray();
                    ScriptCompilationCache.Store(hash, ilBytes);
                }

                _assembly = Assembly.Load(ilBytes);
                var type = _assembly.GetTypes().FirstOrDefault(t => typeof(IFtDSharp).IsAssignableFrom(t));
                if (type == null)
                {
                    _lastError = "No class implementing IFtDSharp found. Your script must have a class that implements IFtDSharp.";
                    _instance = null;
                    _hash = null;
                    return (false, diagnosticsList.ToArray());
                }

                // Push context before instantiation so Game/Logging APIs are available in constructor
                using (ctx != null ? ScriptApi.PushContext(ctx) : null)
                {
                    _instance = (IFtDSharp)Activator.CreateInstance(type)!;
                }

                LastCompileTime = sw.Elapsed;
                _hash = hash;
                return (true, diagnosticsList.ToArray());
            }
            catch (Exception ex)
            {
                _lastError = $"Error loading script: {ex.Message}";
                _instance = null;
                _assembly = null;
                _hash = null;
                return (false, diagnosticsList.ToArray());
            }
        }

        internal bool TryCompileAndActivate(string code, IScriptContext? ctx, out (string Hash, TimeSpan CompileTime, Diagnostic[] Diagnostics, string? ErrorMessage) diagnostics)
        {
            var hash = ComputeHash(code);

            var (success, diags) = LoadScript(code, hash, ctx);

            if (success)
            {
                diagnostics = (hash, LastCompileTime, diags, null);
                _startUtc = DateTime.UtcNow;
                return true;
            }

            diagnostics = (hash, TimeSpan.Zero, diags, _lastError);
            return false;
        }

        internal void Tick(IScriptContext ctx, float deltaTime)
        {
            if (_instance == null) return;

            using var scope = ScriptApi.PushContext(ctx);

            var profile = AbstractModule<FtDSharpProfiler>.Instance.ScriptExecution;
            var startTime = profile.Start();
            try
            {
                _instance.Update(deltaTime);
            }
            catch (Exception ex)
            {
                ctx.Log.Error($"Error during script execution: {ex.Message}\n{ex.StackTrace}");
                _instance = null; // deactivate on error
            }
            finally
            {
                profile.Finish(startTime);
            }
        }

        public void Deactivate()
        {
            _instance = null;
            _assembly = null;
            _hash = null;
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

        private static string ComputeHash(string input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
