using System;

namespace HandyScreenshot.Common
{
    public delegate void RectChangedEventHandler(double x, double y, double width, double height);

    public class RectProxy
    {
        public event RectChangedEventHandler? RectChanged;

        public double X { get; private set; }

        public double Y { get; private set; }

        public double Width { get; private set; }

        public double Height { get; private set; }

        public bool Contains(double x, double y) => X <= x && Y <= y && x <= X + Width && y <= Y + Height;

        public void Offset(double x1, double y1, double x2, double y2)
        {
            X = X + x2 - x1;
            Y = Y + y2 - y1;
            OnRectChanged();
        }

        public void Union(double x, double y)
        {
            Width = Math.Max(Width, X < x ? x - X : X - x + Width);
            Height = Math.Max(Height, Y < y ? y - Y : Y - y + Height);
            X = Math.Min(X, x);
            Y = Math.Min(Y, y);
            OnRectChanged();
        }

        public void Set(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            OnRectChanged();
        }

        public void SetLeft(double left)
        {
            if (left > X + Width) return;

            Width = X + Width - left;
            X = left;
            OnRectChanged();
        }

        public void SetRight(double right)
        {
            if (right < X) return;

            Width = right - X;
            OnRectChanged();
        }

        public void SetTop(double top)
        {
            if (top > Y + Height) return;

            Height = Y + Height - top;
            Y = top;
            OnRectChanged();
        }

        public void SetBottom(double bottom)
        {
            if (bottom < Y) return;

            Height = bottom - Y;
            OnRectChanged();
        }

        protected virtual void OnRectChanged() => RectChanged?.Invoke(X, Y, Width, Height);
    }
}
