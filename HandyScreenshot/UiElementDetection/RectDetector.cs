using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation;

namespace HandyScreenshot.UiElementDetection
{
    public class RectDetector
    {
        [DebuggerDisplay("{" + nameof(PhysicalRect) + "}")]
        private class CachedRect
        {
            public AutomationElement Element { get; }

            public Rect PhysicalRect { get; }

            public IReadOnlyList<CachedRect> Children { get; set; }

            public CachedRect(AutomationElement element, Rect physicalRect)
            {
                Element = element;
                PhysicalRect = physicalRect;
            }
        }

        private static readonly IReadOnlyList<CachedRect> EmptyChildren = Enumerable.Empty<CachedRect>().ToList();
        private static readonly System.Windows.Automation.Condition ChildrenCondition
            = new NotCondition(new PropertyCondition(WindowPattern.WindowVisualStateProperty, WindowVisualState.Minimized));

        private IReadOnlyList<CachedRect> _elementSnapshot;

        public void Snapshot(Rect physicalFullScreenRect)
        {
            _elementSnapshot = GetChildren(AutomationElement.RootElement, physicalFullScreenRect);
        }

        public Rect GetByPhysicalPoint(Point physicalPoint)
        {
            if (_elementSnapshot == null)
                throw new InvalidOperationException("");

            return GetAdjustRect(_elementSnapshot, physicalPoint)?.PhysicalRect ?? Rect.Empty;
        }

        private static CachedRect GetAdjustRect(IReadOnlyCollection<CachedRect> elements, Point physicalPoint)
        {
            CachedRect result = null;

            while (true)
            {
                var temp = elements.FirstOrDefault(item => item.PhysicalRect.Contains(physicalPoint));

                if (temp == null) break;

                result = temp;
                if (EnsureChildren(result))
                {
                    elements = result.Children;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private static bool EnsureChildren(CachedRect cachedRect)
        {
            if (cachedRect.Children == null)
            {
                cachedRect.Children = GetChildren(cachedRect.Element, cachedRect.PhysicalRect);
            }

            return cachedRect.Children.Count > 0;
        }

        private static IReadOnlyList<CachedRect> GetChildren(AutomationElement parentElement, Rect physicalParentRect)
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
                .Select(item => new CachedRect(item.element, item.rect))
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
