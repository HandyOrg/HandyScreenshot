using System;

namespace HandyScreenshot
{
    public sealed class Disposable : IDisposable
    {
        public static IDisposable Create(Action action) => new Disposable(action);

        private readonly Action _action;

        public Disposable(Action action) => _action = action;

        public void Dispose() => _action?.Invoke();
    }
}
