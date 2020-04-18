using System;
using System.Collections.Generic;

namespace HandyScreenshot.Common
{
    public static class SharedProperties
    {
        public static Queue<IDisposable> Disposables { get; } = new Queue<IDisposable>();
    }
}
