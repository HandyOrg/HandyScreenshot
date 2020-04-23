using System;
using System.Collections.Generic;

namespace HandyScreenshot.Common
{
    public static class SharedProperties
    {
        public static Stack<IDisposable> Disposables { get; } = new Stack<IDisposable>();
    }
}
