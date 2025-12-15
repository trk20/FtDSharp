using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using UnityEngine;

namespace FtDSharp
{
    public class ScriptHost
    {
        private readonly BasicLogApi _log = new BasicLogApi();
        private IFtDSharp? _instance;
        private Assembly? _assembly;
        private DateTime _startUtc;
        private string? _hash;
        private static readonly MetadataReference[] DefaultReferences = BuildDefaultReferences();
        private const string ScriptPrelude = "#line 1\n";

        public bool Active => _instance != null;
        public string? CurrentHash => _hash;
        public TimeSpan LastCompileTime { get; private set; }

        internal void LoadScript(string code, string hash, IScriptContext? ctx = null)
        {
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

                // Validate script doesn't use forbidden namespaces
                var validationErrors = ForbiddenNamespaceValidator.GetInvalidUsages(compilation, syntaxTree);
                if (validationErrors.Count > 0)
                {
                    throw new InvalidOperationException("Forbidden namespace usage:\n" + string.Join("\n", validationErrors));
                }

                using var ms = new MemoryStream();
                EmitResult emitResult = compilation.Emit(ms);
                if (!emitResult.Success)
                {
                    var diag = string.Join("\n", emitResult.Diagnostics.Select(d => d.ToString()));
                    throw new InvalidOperationException("Compilation failed:\n" + diag);
                }

                ms.Seek(0, SeekOrigin.Begin);
                _assembly = Assembly.Load(ms.ToArray());
                var type = _assembly.GetTypes().FirstOrDefault(t => typeof(IFtDSharp).IsAssignableFrom(t)) ?? throw new Exception("No class implementing IFtDSharp found.");

                // Push context before instantiation so Game/Logging APIs are available in constructor
                using (ctx != null ? ScriptApi.PushContext(ctx) : null)
                {
                    _instance = (IFtDSharp)Activator.CreateInstance(type)!;
                }

                LastCompileTime = sw.Elapsed;
                _hash = hash;

            }
            catch (Exception ex)
            {
                _log.Error("Error loading script: " + ex.Message + "\n " + ex.StackTrace ?? "");
                _instance = null;
                _assembly = null;
                _hash = null;
            }
        }

        internal bool TryCompileAndActivate(string code, IScriptContext ctx, out (string Hash, TimeSpan CompileTime, Diagnostic[] Diagnostics) diagnostics)
        {
            var hash = ComputeHash(code);
            diagnostics = (hash, TimeSpan.Zero, Array.Empty<Diagnostic>());

            LoadScript(code, hash, ctx);
            if (_instance != null)
            {
                diagnostics = (hash, LastCompileTime, Array.Empty<Diagnostic>());
                _startUtc = DateTime.UtcNow;
                Tick(ctx, 0f);
                return true;
            }
            return false;
        }

        internal void Tick(IScriptContext ctx, float deltaTime)
        {
            if (_instance == null) return;

            using var scope = ScriptApi.PushContext(ctx);

            try
            {
                _instance.Update(deltaTime);
            }
            catch (Exception ex)
            {
                _log.Error("Error during script execution: " + ex.Message);
                _instance = null; // deactivate on error
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
