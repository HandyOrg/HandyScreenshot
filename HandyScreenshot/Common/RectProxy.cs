using System;

namespace HandyScreenshot.Common
{
    public delegate void RectChangedEventHandler(in int x, in int y, in int width, in int height);

    public class RectProxy
    {
        public event RectChangedEventHandler? RectChanged;

        public int X { get; private set; }

        public int Y { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public bool Contains(in int x, in int y) => X <= x && Y <= y && x <= X + Width && y <= Y + Height;

        public void Offset(in int x1, in int y1, in int x2, in int y2)
        {
            X = X + x2 - x1;
            Y = Y + y2 - y1;
            OnRectChanged();
        }

        public void Union(in int x, in int y)
        {
            Width = Math.Max(Width, X < x ? x - X : X - x + Width);
            Height = Math.Max(Height, Y < y ? y - Y : Y - y + Height);
            X = Math.Min(X, x);
            Y = Math.Min(Y, y);
            OnRectChanged();
        }

        public void Set(in int x, in int y, in int width, in int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            OnRectChanged();
        }

        public void SetLeft(in int left)
        {
            if (left > X + Width) return;

            Width = X + Width - left;
            X = left;
            OnRectChanged();
        }

        public void SetRight(in int right)
        {
            if (right < X) return;

            Width = right - X;
            OnRectChanged();
        }

        public void SetTop(in int top)
        {
            if (top > Y + Height) return;

            Height = Y + Height - top;
            Y = top;
            OnRectChanged();
        }

        public void SetBottom(in int bottom)
        {
            if (bottom < Y) return;

            Height = bottom - Y;
            OnRectChanged();
        }

        protected virtual void OnRectChanged() => RectChanged?.Invoke(X, Y, Width, Height);
    }
}
