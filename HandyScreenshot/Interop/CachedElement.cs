using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using Condition = System.Windows.Automation.Condition;

namespace HandyScreenshot.Interop
{
    [DebuggerDisplay("{Info.ClassName}, {Info.Name}")]
    public class CachedElement
    {
        private IReadOnlyList<CachedElement> _children;

        private readonly AutomationElement _element;

        public AutomationElement.AutomationElementInformation Info { get; }

        public Rect Rect { get; private set; }

        public IReadOnlyList<CachedElement> Children => _children ??= GetChildren(_element, Constants.ScaleFactor);

        public CachedElement(AutomationElement element)
        {
            _element = element;
            Info = element.Current;
        }

        public static IReadOnlyList<CachedElement> GetChildren(AutomationElement element, double scale)
        {
            // BUG: System.Runtime.InteropServices.COMException:
            // 'An outgoing call cannot be made since the application is dispatching an input-synchronous call.
            // (Exception from HRESULT: 0x8001010D (RPC_E_CANTCALLOUT_ININPUTSYNCCALL))'
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
                    return rect != Constants.RectZero ? new CachedElement(item) { Rect = rect } : null;
                })
                .Where(item => item != null)
                .ToList();
        }

        private static Rect GetRect(AutomationElement element, double scale)
        {
            try
            {
                var rect = element.Current.BoundingRectangle;
                return rect != Rect.Empty ? rect.Scale(scale) : Constants.RectZero;
            }
            catch
            {
                return Constants.RectZero;
            }
        }
    }
}
