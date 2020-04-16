using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using Condition = System.Windows.Automation.Condition;

namespace HandyScreenshot.UiElementDetection
{
    [DebuggerDisplay("{" + nameof(PhysicalRect) + "}")]
    public class CachedRect
    {
        //private const double MinRectLimit = 32 * 32;
        private static readonly IReadOnlyList<CachedRect> EmptyChildren = Enumerable.Empty<CachedRect>().ToList();
        private static readonly Condition ChildrenCondition
            = new NotCondition(new PropertyCondition(WindowPattern.WindowVisualStateProperty, WindowVisualState.Minimized));

        private IReadOnlyList<CachedRect> _children;

        private readonly AutomationElement _element;

        public Rect PhysicalRect { get; private set; }

        public IReadOnlyList<CachedRect> Children => _children ??= GetChildren(_element, PhysicalRect);

        public CachedRect(AutomationElement element) => _element = element;

        internal static IReadOnlyList<CachedRect> GetChildren(AutomationElement parentElement, Rect physicalParentRect)
        {
            try
            {
                return CriticalGetChildren(parentElement, physicalParentRect);
            }
            catch
            {
                // Ignore Exception

                // * System.Runtime.InteropServices.COMException:
                // 'An outgoing call cannot be made since the application is dispatching an input-synchronous call.
                // (Exception from HRESULT: 0x8001010D (RPC_E_CANTCALLOUT_ININPUTSYNCCALL))'
                // * System.ArgumentException:
                // 'Value does not fall within the expected range.'
                // * System.Windows.Automation.ElementNotAvailableException:
                // 'The target element corresponds to UI that is no longer available (for example, the parent window has closed).'
                // * System.Runtime.InteropServices.COMException:
                // 'Error HRESULT E_FAIL has been returned from a call to a COM component.'

                return EmptyChildren;
            }
        }

        private static IReadOnlyList<CachedRect> CriticalGetChildren(AutomationElement parentElement, Rect physicalParentRect)
        {
            return parentElement.FindAll(TreeScope.Children, ChildrenCondition)
                .OfType<AutomationElement>()
                .Select(item => (element: item, rect: GetRect(item, physicalParentRect)))
                .Where(item => item.rect != Rect.Empty)
                //.Where(item => item.PhysicalRect.Width * item.PhysicalRect.Height > MinRectLimit)
                .Select(item => new CachedRect(item.element) { PhysicalRect = item.rect })
                .ToList();
        }

        private static Rect GetRect(AutomationElement element, Rect parentRect)
        {
            try
            {
                var rect = element.Current.BoundingRectangle;
                if (rect == Rect.Empty) return Rect.Empty;

                rect.Intersect(parentRect);
                return rect;
            }
            catch
            {
                return Rect.Empty;
            }
        }
    }
}
