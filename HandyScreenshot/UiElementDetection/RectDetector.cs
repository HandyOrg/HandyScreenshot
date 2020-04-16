using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;

namespace HandyScreenshot.UiElementDetection
{
    public class RectDetector
    {
        private IReadOnlyList<CachedRect> _elementSnapshot;

        public void Snapshot(Rect physicalFullScreenRect)
        {
            _elementSnapshot = CachedRect.GetChildren(AutomationElement.RootElement, physicalFullScreenRect);
        }

        public Rect GetByPhysicalPoint(Point physicalPoint)
        {
            if (_elementSnapshot == null)
                throw new InvalidOperationException("");

            return GetAdjustElement(_elementSnapshot, physicalPoint)?.PhysicalRect ?? Rect.Empty;
        }

        private static CachedRect GetAdjustElement(IReadOnlyCollection<CachedRect> elements, Point physicalPoint)
        {
            CachedRect result = null;

            while (true)
            {
                var temp = elements
                    .FirstOrDefault(item => item.PhysicalRect.Contains(physicalPoint));

                if (temp == null) break;

                result = temp;
                var children = result.Children;
                if (children.Count > 0)
                {
                    elements = children;
                }
                else
                {
                    break;
                }
            }

            return result;
        }
    }
}
