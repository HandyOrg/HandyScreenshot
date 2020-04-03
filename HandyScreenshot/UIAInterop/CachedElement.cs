using HandyScreenshot.Interop;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using Condition = System.Windows.Automation.Condition;

namespace HandyScreenshot.UIAInterop
{
    public class CachedElement
    {
        private IReadOnlyList<CachedElement> _children;

        private readonly AutomationElement _element;

        public AutomationElement.AutomationElementInformation Info { get; }

        public Rect Rect { get; private set; }

        public IReadOnlyList<CachedElement> Children => _children ??= GetChildren(_element);

        public CachedElement(AutomationElement element)
        {
            _element = element;
            Info = element.Current;
        }

        public static IReadOnlyList<CachedElement> GetChildren(AutomationElement element)
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
                    var rect = GetRect(item);
                    return rect != Rect.Empty ? new CachedElement(item) { Rect = rect } : null;
                })
                .Where(item => item != null)
                .ToList();
        }

        private static Rect GetRect(AutomationElement element)
        {
            try
            {
                return element.Current.BoundingRectangle.Scale(0.8);
            }
            catch
            {
                return Rect.Empty;
            }
        }
    }
}
