using FtDSharp.Tests.Helpers;
using FtDSharp.Tests.Mocks;
using Microsoft.CodeAnalysis;
using Xunit;

namespace FtDSharp.Tests;

public class CompilationTests
{
    private const string _entryPointValidationError = "must be a public instance parameterless void method.";

    [Fact]
    public void DefaultTemplate_Compiles()
    {
        AssertCompiles(ReplaceLuaPatches.DefaultCSharpTemplate);
    }

    [Fact]
    public void MinimalScript_Compiles()
    {
        const string code = """
            public class MinimalScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                }
            }
            """;

        (ScriptHost host, var hash, Diagnostic[] diagnostics) = Compile(code);

        Assert.Null(host.LastError);
        Assert.Empty(diagnostics);
        Assert.True(host.Instantiate(hash, new TestProviderScope()));
    }

    [Fact]
    public void AllAttributes_Compile()
    {
        const string code = """
            public class FullLifecycleScript
            {
                [OnStart]
                public void Start()
                {
                    Log("start");
                }

                [OnPhysicsTick]
                public void Tick()
                {
                    Log("tick");
                }

                [OnStop]
                public void Stop()
                {
                    Log("stop");
                }
            }
            """;

        var scope = new TestProviderScope();
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        host.Tick(scope);
        host.Deactivate();

        Assert.Equal(new[] { "start", "tick", "stop" }, scope.LogProvider.InfoMessages);
    }

    [Fact]
    public void MultipleClasses_ClassWithAttributes_Selected()
    {
        const string code = """
            public class NoAttributes
            {
                public void Foo() { }
            }

            public class HasAttributes
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    Log("correct");
                }
            }
            """;

        var scope = new TestProviderScope();
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        host.Tick(scope);

        Assert.Contains("correct", scope.LogProvider.InfoMessages);
    }

    [Fact]
    public void Script_CanUseList_WithoutImport()
    {
        const string code = """
            public class ListScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    var values = new List<int> { 1, 2, 3 };
                    Log(values.Count.ToString());
                }
            }
            """;

        AssertCompiles(code);
    }

    [Fact]
    public void Script_CanUseLinq_WithoutImport()
    {
        const string code = """
            public class LinqScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    var values = new[] { 1, 2, 3 }.Where(x => x > 1).ToList();
                    Log(values.Count.ToString());
                }
            }
            """;

        AssertCompiles(code);
    }

    [Fact]
    public void Script_CanUseVector3_WithoutImport()
    {
        const string code = """
            public class VectorScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    var vector = new Vector3(1f, 2f, 3f);
                    Log(vector.magnitude.ToString());
                }
            }
            """;

        AssertCompiles(code);
    }

    [Fact]
    public void Script_CanCallLog_WithoutPrefix()
    {
        const string code = """
            public class LoggingScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    Log("message");
                }
            }
            """;

        var scope = new TestProviderScope();
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        host.Tick(scope);

        Assert.Equal(new[] { "message" }, scope.LogProvider.InfoMessages);
    }

    [Fact]
    public void Script_CanAccessGameClass_WithNamespace()
    {
        const string code = """
            public class GameScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    Log(Game.GameTime.ToString());
                }
            }
            """;

        var scope = new TestProviderScope();
        scope.GameProvider.GameTime = 42.5f;
        ScriptHost host = ScriptTestHelper.CompileAndInstantiate(code, scope);

        host.Tick(scope);

        Assert.Equal(new[] { "42.5" }, scope.LogProvider.InfoMessages);
    }

    [Fact]
    public void FileAccess_Rejected()
    {
        const string code = """
            public class FileScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    _ = System.IO.File.Exists("test.txt");
                }
            }
            """;

        AssertCompileFailsWithMessage(code, "File system access is not allowed.");
    }

    [Fact]
    public void NetworkAccess_Rejected()
    {
        const string code = """
            public class NetworkScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    _ = new System.Net.NetworkCredential("user", "password");
                }
            }
            """;

        AssertCompileFailsWithMessage(code, "Direct network access is not allowed.");
    }

    [Fact]
    public void Reflection_Rejected()
    {
        const string code = """
            public class ReflectionScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    _ = typeof(string).Assembly.GetName().Name;
                }
            }
            """;

        AssertCompileFailsWithMessage(code, "Reflection is forbidden to prevent sandbox escapes.");
    }

    [Fact]
    public void ThreadCreation_Rejected()
    {
        const string code = """
            public class ThreadScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    _ = new System.Threading.Thread(() => { });
                }
            }
            """;

        AssertCompileFailsWithMessage(code, "Direct thread creation is not allowed.");
    }

    [Fact]
    public void ProcessExecution_Rejected()
    {
        const string code = """
            public class DiagnosticsScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    _ = System.Diagnostics.Process.GetCurrentProcess();
                }
            }
            """;

        AssertCompileFailsWithMessage(code, "Process execution is not allowed.");
    }

    [Fact]
    public void Activator_Rejected()
    {
        const string code = """
            public class ActivatorScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    _ = Activator.CreateInstance(typeof(object));
                }
            }
            """;

        (var success, ScriptHost host) = ScriptTestHelper.TryCompile(code);

        Assert.False(success);
        Assert.Contains("Dynamic object creation via Activator is not allowed", host.LastError);
    }

    [Fact]
    public void TypeGetType_Rejected()
    {
        const string code = """
            public class TypeGetTypeScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    _ = Type.GetType("System.String");
                }
            }
            """;

        (var success, ScriptHost host) = ScriptTestHelper.TryCompile(code);

        Assert.False(success);
        Assert.Contains("Type loading by name is not allowed", host.LastError);
    }

    [Fact]
    public void Environment_Rejected()
    {
        const string code = """
            public class EnvironmentScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    _ = Environment.GetEnvironmentVariable("PATH");
                }
            }
            """;

        (var success, ScriptHost host) = ScriptTestHelper.TryCompile(code);

        Assert.False(success);
        Assert.Contains("Environment variable access is not allowed", host.LastError);
    }

    [Fact]
    public void Parallel_Rejected()
    {
        const string code = """
            public class ParallelScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    System.Threading.Tasks.Parallel.For(0, 10, i => { });
                }
            }
            """;

        (var success, ScriptHost host) = ScriptTestHelper.TryCompile(code);

        Assert.False(success);
        Assert.Contains("Arbitrary parallelism is not allowed", host.LastError);
    }

    [Fact]
    public void InteropServices_Rejected()
    {
        const string code = """
            public class InteropScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    _ = System.Runtime.InteropServices.Marshal.SizeOf<int>();
                }
            }
            """;

        (var success, ScriptHost host) = ScriptTestHelper.TryCompile(code);

        Assert.False(success);
        Assert.Contains("Native interop is not allowed", host.LastError);
    }

    [Fact]
    public void DynamicKeyword_Rejected()
    {
        const string code = """
            public class DynamicScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    dynamic value = "hello";
                    _ = value.Length;
                }
            }
            """;

        (var success, ScriptHost host) = ScriptTestHelper.TryCompile(code);

        Assert.False(success);
        Assert.Contains("Dynamic", host.LastError);
    }

    [Fact]
    public void ExpressionCompile_Rejected()
    {
        const string code = """
            public class ExpressionScript
            {
                [OnPhysicsTick]
                public void Tick()
                {
                    System.Linq.Expressions.Expression<System.Func<int>> expression = () => 1;
                    _ = expression.Compile();
                }
            }
            """;

        (var success, ScriptHost host) = ScriptTestHelper.TryCompile(code);

        Assert.False(success);
        Assert.Contains("Dynamic compilation is not allowed", host.LastError);
    }

    [Fact]
    public void UnsafeCode_Rejected()
    {
        const string code = """
            public class UnsafeScript
            {
                [OnPhysicsTick]
                public unsafe void Tick()
                {
                    int* pointer = null;
                }
            }
            """;

        (var success, ScriptHost host) = ScriptTestHelper.TryCompile(code);

        Assert.False(success);
        Assert.Contains("unsafe code", host.LastError, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SyntaxError_ReportsCorrectLine()
    {
        const string code = """
            public class BrokenScript
            {
                private int value =
            }
            """;

        (ScriptHost host, var _, Diagnostic[] diagnostics) = Compile(code, expectSuccess: false);
        Diagnostic[] errorDiagnostics = diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error).ToArray();

        Assert.False(string.IsNullOrEmpty(host.LastError));
        Assert.NotEmpty(errorDiagnostics);
        Assert.All(errorDiagnostics, diagnostic =>
        {
            FileLinePositionSpan mappedLineSpan = diagnostic.Location.GetMappedLineSpan();
            Assert.Equal(3, mappedLineSpan.StartLinePosition.Line + 1);
        });
    }

    [Fact]
    public void NoAttributes_FailsInstantiation()
    {
        const string code = """
            public class NoEntryPointsScript
            {
                public void Tick()
                {
                }
            }
            """;

        AssertInstantiationFailsWithMessage(
            code,
            "No public class with entry point methods found.");
    }

    [Fact]
    public void PrivateMethod_WithAttribute_FailsInstantiation()
    {
        const string code = """
            public class PrivateEntryPointScript
            {
                [OnPhysicsTick]
                private void Tick()
                {
                }
            }
            """;

        AssertInstantiationFailsWithMessage(code, _entryPointValidationError);
    }

    [Fact]
    public void NonVoidMethod_WithAttribute_FailsInstantiation()
    {
        const string code = """
            public class NonVoidEntryPointScript
            {
                [OnPhysicsTick]
                public int Tick()
                {
                    return 1;
                }
            }
            """;

        AssertInstantiationFailsWithMessage(code, _entryPointValidationError);
    }

    [Fact]
    public void MethodWithParameters_WithAttribute_FailsInstantiation()
    {
        const string code = """
            public class ParameterizedEntryPointScript
            {
                [OnPhysicsTick]
                public void Tick(int frame)
                {
                }
            }
            """;

        AssertInstantiationFailsWithMessage(code, _entryPointValidationError);
    }

    private static void AssertCompiles(string code)
    {
        (ScriptHost host, var _, Diagnostic[] diagnostics) = Compile(code);

        Assert.Null(host.LastError);
        Assert.Empty(diagnostics);
    }

    private static void AssertCompileFailsWithMessage(string code, string expectedMessage)
    {
        (ScriptHost host, var _, Diagnostic[] diagnostics) = Compile(code, expectSuccess: false);

        Assert.NotEmpty(diagnostics);
        Assert.False(string.IsNullOrWhiteSpace(host.LastError));
        Assert.Contains(expectedMessage, host.LastError);
    }

    private static void AssertInstantiationFailsWithMessage(string code, string expectedMessage)
    {
        ScriptHost host = ScriptTestHelper.Compile(code);
        var hash = ScriptHost.ComputeHash(code);
        var instantiated = host.Instantiate(hash, new TestProviderScope());

        Assert.False(instantiated);
        Assert.False(string.IsNullOrWhiteSpace(host.LastError));
        Assert.Contains(expectedMessage, host.LastError);
    }

    private static (ScriptHost Host, string Hash, Diagnostic[] Diagnostics) Compile(string code, bool expectSuccess = true)
    {
        var host = new ScriptHost();
        var hash = ScriptHost.ComputeHash(code);
        (var success, Diagnostic[] diagnostics) = host.Compile(code, hash);

        Assert.Equal(expectSuccess, success);

        return (host, hash, diagnostics);
    }
}