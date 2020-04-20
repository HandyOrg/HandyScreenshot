using System;

namespace HandyScreenshot.Controls
{
    public class RectOperation
    {
        public double X { get; private set; }

        public double Y { get; private set; }

        public double Width { get; private set; }

        public double Height { get; private set; }

        private Action<double> _setX = _ => { };
        private Action<double> _setY = _ => { };
        private Action<double> _setWidth = _ => { };
        private Action<double> _setHeight = _ => { };

        public void Attach(Action<double> setX, Action<double> setY, Action<double> setWidth, Action<double> setHeight)
        {
            _setX += x => setX(X = x);
            _setY += y => setY(Y = y);
            _setWidth += w => setWidth(Width = w);
            _setHeight += h => setHeight(Height = h);
        }

        public bool Contains(double x, double y) => X <= x && Y <= y && x <= X + Width && y <= Y + Height;

        public void Offset(double x1, double y1, double x2, double y2)
        {
            _setX(X + x2 - x1);
            _setY(Y + y2 - y1);
        }

        public void Union(double x, double y)
        {
            _setWidth(Math.Max(Width, X < x ? x - X : X - x + Width));
            _setHeight(Math.Max(Height, Y < y ? y - Y : Y - y + Height));
            _setX(Math.Min(X, x));
            _setY(Math.Min(Y, y));
        }

        public void Set(double x, double y, double width, double height)
        {
            _setX(x);
            _setY(y);
            _setWidth(width);
            _setHeight(height);
        }

        public void SetLeft(double left)
        {
            _setWidth(X - left + Width);
            _setX(left);
        }

        public void SetRight(double right) => _setWidth(right - X);

        public void SetTop(double top)
        {
            _setHeight(Y - top + Height);
            _setY(top);
        }

        public void SetBottom(double bottom) => _setHeight(bottom - Y);
    }
}
