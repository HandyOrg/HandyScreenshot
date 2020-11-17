using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Automation;
using HandyScreenshot.Common;
using Condition = System.Windows.Automation.Condition;

namespace HandyScreenshot.Detection
{
    public class RectDetector
    {
        [DebuggerDisplay("{" + nameof(PhysicalRect) + "}")]
        private class CachedRect
        {
            public AutomationElement Element { get; }

            public ReadOnlyRect PhysicalRect { get; }

            public IReadOnlyList<CachedRect>? Children { get; set; }

            public CachedRect(AutomationElement element, ReadOnlyRect physicalRect)
            {
                Element = element;
                PhysicalRect = physicalRect;
            }
        }

        private static readonly IReadOnlyList<CachedRect> EmptyChildren =
            Enumerable.Empty<CachedRect>().ToList().AsReadOnly();

        private static readonly Condition ChildrenCondition
            = new NotCondition(new PropertyCondition(WindowPattern.WindowVisualStateProperty,
                WindowVisualState.Minimized));

        private IReadOnlyList<CachedRect> _elementSnapshot = EmptyChildren;

        public void Snapshot(ReadOnlyRect physicalFullScreenRect)
        {
            _elementSnapshot = GetChildren(AutomationElement.RootElement, physicalFullScreenRect);
        }

        public ReadOnlyRect GetByPhysicalPoint(double physicalX, double physicalY)
        {
            return GetAdjustRect(_elementSnapshot, physicalX, physicalY)?.PhysicalRect ?? ReadOnlyRect.Empty;
        }

        private static CachedRect? GetAdjustRect(
            IReadOnlyCollection<CachedRect> elements,
            double physicalX,
            double physicalY)
        {
            CachedRect? result = null;

            while (true)
            {
                var temp = elements.FirstOrDefault(item => item.PhysicalRect.Contains(physicalX, physicalY));

                if (temp == null) break;

                result = temp;
                if (EnsureChildren(result))
                {
                    elements = result.Children!;
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
            cachedRect.Children ??= GetChildren(cachedRect.Element, cachedRect.PhysicalRect);

            return cachedRect.Children.Count > 0;
        }

        private static IReadOnlyList<CachedRect> GetChildren(AutomationElement parentElement,
            ReadOnlyRect physicalParentRect)
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
                // (Exception from HRESULT: 0x8001010D (RPC_E_CANT CALL OUT_IN INPUT SYNC CALL))'
                // * System.ArgumentException:
                // 'Value does not fall within the expected range.'
                // * System.Windows.Automation.ElementNotAvailableException:
                // 'The target element corresponds to UI that is no longer available (for example, the parent window has closed).'
                // * System.Runtime.InteropServices.COMException:
                // 'Error HRESULT E_FAIL has been returned from a call to a COM component.'

                return EmptyChildren;
            }
        }

        private static IReadOnlyList<CachedRect> CriticalGetChildren(
            AutomationElement parentElement,
            ReadOnlyRect physicalParentRect)
        {
            return parentElement.FindAll(TreeScope.Children, ChildrenCondition)
                .OfType<AutomationElement>()
                .Select(item => (element: item, rect: GetRect(item, physicalParentRect)))
                .Where(item => item.rect != ReadOnlyRect.Empty)
                .Select(item => new CachedRect(item.element, item.rect))
                .ToList();
        }

        private static ReadOnlyRect GetRect(AutomationElement element, ReadOnlyRect physicalParentRect)
        {
            try
            {
                ReadOnlyRect rect = element.Current.BoundingRectangle;
                return rect == ReadOnlyRect.Empty ? ReadOnlyRect.Empty : rect.Intersect(physicalParentRect);
            }
            catch
            {
                return ReadOnlyRect.Empty;
            }
        }
    }
}