using System;
using System.Windows;
using System.Windows.Media;

namespace HandyScreenshot.Controls
{
    public abstract class DrawingControlBase : FrameworkElement
    {
        private readonly DrawingVisual _drawingVisual;

        protected DrawingControlBase()
        {
            var children = new VisualCollection(this) { new DrawingVisual() };
            _drawingVisual = (DrawingVisual)children[0];
        }

        protected virtual void RefreshDrawingVisual()
        {
            var dc = _drawingVisual.RenderOpen();
            DrawingOverride(dc);
            dc.Close();
        }

        protected abstract void DrawingOverride(DrawingContext dc);

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException();

            return _drawingVisual;
        }
    }
}
