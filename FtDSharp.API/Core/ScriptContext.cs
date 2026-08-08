using System;
using System.Threading;

namespace FtDSharp
{
    public static class ScriptContext
    {
        private static readonly AsyncLocal<IProviderScope?> Scope = new();

        internal static IProviderScope? Current
        {
            get => Scope.Value;
            set => Scope.Value = value;
        }

        public static IDisposable Push(IProviderScope scope)
        {
            IProviderScope? previous = Scope.Value;
            Scope.Value = scope;
            return new ScopeGuard(previous);
        }

        private sealed class ScopeGuard : IDisposable
        {
            private readonly IProviderScope? _previous;

            public ScopeGuard(IProviderScope? previous)
            {
                _previous = previous;
            }

            public void Dispose()
            {
                Scope.Value = _previous;
            }
        }
    }
}
