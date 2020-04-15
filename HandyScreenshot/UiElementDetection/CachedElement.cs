using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using HandyScreenshot.Helpers;
using Condition = System.Windows.Automation.Condition;

namespace HandyScreenshot.UiElementDetection
{
    [DebuggerDisplay("{Info.ClassName}, {Info.Name}")]
    public class CachedElement
    {
        private static readonly IReadOnlyList<CachedElement> EmptyChildren = Enumerable.Empty<CachedElement>().ToList();

        private IReadOnlyList<CachedElement> _children;

        private readonly AutomationElement _element;

        public AutomationElement.AutomationElementInformation Info { get; }

        public Rect PhysicalRect { get; private set; }

        public IReadOnlyList<CachedElement> Children => _children ??= GetChildren(_element);

        public CachedElement(AutomationElement element)
        {
            _element = element;
            Info = element.Current;
        }

        internal static IReadOnlyList<CachedElement> GetChildren(AutomationElement element)
        {
            try
            {
                return CriticalGetChildren(element);
            }
            catch
            {
                // Ignore Exception

                // BUG: System.Runtime.InteropServices.COMException:
                // 'An outgoing call cannot be made since the application is dispatching an input-synchronous call.
                // (Exception from HRESULT: 0x8001010D (RPC_E_CANTCALLOUT_ININPUTSYNCCALL))'
                // BUG: System.ArgumentException:
                // 'Value does not fall within the expected range.'
                // BUG: System.Windows.Automation.ElementNotAvailableException:
                // 'The target element corresponds to UI that is no longer available (for example, the parent window has closed).'
                // BUG: System.Runtime.InteropServices.COMException:
                // 'Error HRESULT E_FAIL has been returned from a call to a COM component.'

                return EmptyChildren;
            }
        }

        private static IReadOnlyList<CachedElement> CriticalGetChildren(AutomationElement element)
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

                    var rect = GetRect(element, item);
                    return rect != Constants.RectZero ? new CachedElement(item) { PhysicalRect = rect } : null;
                })
                .Where(item => item != null)
                .ToList();
        }

        private static Rect GetRect(AutomationElement parent, AutomationElement element)
        {
            try
            {
                var rect = element.Current.BoundingRectangle;
                if (rect == Rect.Empty) return Constants.RectZero;

                var parentRect = parent.Current.BoundingRectangle;
                rect.Intersect(parentRect);
                return rect;
            }
            catch
            {
                return Constants.RectZero;
            }
        }
    }
}
