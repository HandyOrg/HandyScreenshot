using HandyScreenshot.Interop;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using Condition = System.Windows.Automation.Condition;

namespace HandyScreenshot.UIAInterop
{
    [DebuggerDisplay("{Info.ClassName}, {Info.Name}")]
    public class CachedElement
    {
        private readonly object _locker = new object();

        private IReadOnlyList<CachedElement> _children;

        private readonly AutomationElement _element;

        public AutomationElement.AutomationElementInformation Info { get; }

        public Rect Rect { get; private set; }

        public IReadOnlyList<CachedElement> Children
        {
            get
            {
                if (_children == null)
                {
                    lock (_locker)
                    {
                        if (_children == null)
                        {
                            _children = GetChildren(_element, MonitorHelper.ScaleFactor);
                        }
                    }
                }

                return _children;
            }
        }

        public CachedElement(AutomationElement element)
        {
            _element = element;
            Info = element.Current;
        }

        public static IReadOnlyList<CachedElement> GetChildren(AutomationElement element, double scale)
        {
            return element.FindAll(TreeScope.Children, Condition.TrueCondition)
                .OfType<AutomationElement>()
                .Select(item =>
                {
                    if (item.GetCurrentPropertyValue(WindowPattern.WindowVisualStateProperty) is WindowVisualState state &&
                        state == WindowVisualState.Minimized)
                    {
                        return null;
                    }

                    var rect = GetRect(item, scale);
                    return rect != Rect.Empty ? new CachedElement(item) { Rect = rect } : null;
                })
                .Where(item => item != null)
                .ToList();
        }

        private static Rect GetRect(AutomationElement element, double scale)
        {
            try
            {
                var rect = element.Current.BoundingRectangle;
                return rect != Rect.Empty ? rect.Scale(scale) : Rect.Empty;
            }
            catch
            {
                return Rect.Empty;
            }
        }
    }
}
