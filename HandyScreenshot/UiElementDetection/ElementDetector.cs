using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;

namespace HandyScreenshot.UiElementDetection
{
    public class ElementDetector
    {
        private IReadOnlyList<CachedElement> _elementSnapshot;

        public void Snapshot(Rect physicalFullScreenRect)
        {
            _elementSnapshot = CachedElement.GetChildren(AutomationElement.RootElement, physicalFullScreenRect);
        }

        public CachedElement GetByPhysicalPoint(Point physicalPoint)
        {
            if (_elementSnapshot == null)
                throw new InvalidOperationException("");

            return GetAdjustElement(_elementSnapshot, physicalPoint);
        }

        private static CachedElement GetAdjustElement(IReadOnlyCollection<CachedElement> elements, Point physicalPoint)
        {
            CachedElement result = null;

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
