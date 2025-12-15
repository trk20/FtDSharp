using System;

namespace FtDSharp
{
    /// <summary>Manages the execution context for scripts.</summary>
    public static class ScriptApi
    {
        [ThreadStatic]
        private static IScriptContext? _current;

        internal static IScriptContext? Context => _current;

        /// <summary>Internal hook used by the runtime to set the active script context.</summary> 
        internal static IDisposable PushContext(IScriptContext ctx) => new ContextScope(ctx);

        private sealed class ContextScope : IDisposable
        {
            private readonly IScriptContext? _previous;

            public ContextScope(IScriptContext ctx)
            {
                _previous = _current;
                _current = ctx;
            }

            public void Dispose()
            {
                _current = _previous;
            }
        }
    }
}