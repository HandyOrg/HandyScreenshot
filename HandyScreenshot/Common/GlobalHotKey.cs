using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using HandyScreenshot.Interop;

namespace HandyScreenshot.Common
{
    public static class GlobalHotKey
    {
        public delegate void Register(ModifierKeys modifier, Key key, Action callback);

        private struct Item
        {
            public ModifierKeys ModifierKeys { get; }

            public Key Key { get; }

            public Action Callback { get; }

            public Item(ModifierKeys modifierKeys, Key key, Action callback)
            {
                ModifierKeys = modifierKeys;
                Key = key;
                Callback = callback;
            }
        }

        private const int WM_HOTKEY = 0x0312;
        private static readonly IntPtr Hwnd = (IntPtr)(-3);
        private static HwndSource _hwndSource;

        public static IDisposable Start(Action<Register> configure)
        {
            // Register
            var items = new Dictionary<(ModifierKeys modifier, Key key), Item>();
            configure?.Invoke(GetRegister(items));

            // Add hook
            var callbackMap = new Dictionary<int, Action>();
            var handler = GetHwndSourceHook(callbackMap);
            AddHook(handler);
            foreach (var item in items.Values)
            {
                callbackMap[item.GetHashCode()] = item.Callback;
                RegisterHotKey(_hwndSource.Handle, item.GetHashCode(), item.ModifierKeys, item.Key);
            }

            return Disposable.Create(() =>
            {
                _hwndSource.RemoveHook(handler);
                foreach (var item in items)
                {
                    UnregisterHotKey(_hwndSource.Handle, item.GetHashCode());
                }
            });
        }

        private static Register GetRegister(IDictionary<(ModifierKeys modifier, Key key), Item> items)
        {
            return (modifier, key, callback) =>
            {
                if (!IsValidKey(key))
                    throw new ArgumentException();

                items[(modifier, key)] = new Item(modifier, key, callback);
            };
        }

        private static HwndSourceHook GetHwndSourceHook(IReadOnlyDictionary<int, Action> callbackMap)
        {
            return (IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
            {
                if (msg != WM_HOTKEY) return IntPtr.Zero;

                try
                {
                    if (callbackMap.TryGetValue(wParam.ToInt32(), out var callback))
                    {
                        callback?.Invoke();
                    }
                }
                catch
                {
                    // TODO: Logging
                }

                return IntPtr.Zero;
            };
        }

        private static void AddHook(HwndSourceHook messageHook)
        {
            const string windowName = "HandyScreenshot";

            if (_hwndSource == null)
            {
                var parameters = new HwndSourceParameters(windowName)
                {
                    HwndSourceHook = messageHook,
                    ParentWindow = Hwnd
                };
                _hwndSource = new HwndSource(parameters);
            }
            else
            {
                _hwndSource.AddHook(messageHook);
            }
        }

        private static void RegisterHotKey(IntPtr hWnd, int id, ModifierKeys modifiers, Key keys)
        {
            var success = NativeMethods.RegisterHotKey(hWnd, id, modifiers, KeyInterop.VirtualKeyFromKey(keys));

            if (!success)
            {
                var errCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errCode);
            }
        }

        private static void UnregisterHotKey(IntPtr hWnd, int id)
        {
            if (_hwndSource != null)
            {
                NativeMethods.UnregisterHotKey(hWnd, id);
            }
        }

        private static bool IsValidKey(Key key) => key >= Key.D0 && key <= Key.Z;
    }
}
