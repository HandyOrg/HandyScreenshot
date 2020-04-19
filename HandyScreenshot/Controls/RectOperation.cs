using System;

namespace HandyScreenshot.Controls
{
    public class RectOperation
    {
        private double _x;
        private double _y;
        private double _width;
        private double _height;

        private Action<double> _setX = _ => { };
        private Action<double> _setY = _ => { };
        private Action<double> _setWidth = _ => { };
        private Action<double> _setHeight = _ => { };

        public void Attach(Action<double> setX, Action<double> setY, Action<double> setWidth, Action<double> setHeight)
        {
            _setX = x => setX(_x = x);
            _setY = y => setY(_y = y);
            _setWidth = w => setWidth(_width = w);
            _setHeight = h => setHeight(_height = h);
        }

        public bool Contains(double x, double y) => _x <= x && _y <= y && x <= _x + _width && y <= _y + _height;

        public void Offset(double x1, double y1, double x2, double y2)
        {
            _setX(_x + x2 - x1);
            _setY(_y + y2 - y1);
        }

        public void Union(double x, double y)
        {
            _setWidth(Math.Max(_width, _x < x ? x - _x : _x - x + _width));
            _setHeight(Math.Max(_height, _y < y ? y - _y : _y - y + _height));
            _setX(Math.Min(_x, x));
            _setY(Math.Min(_y, y));
        }

        public void Set(double x, double y, double width, double height)
        {
            _setX(x);
            _setY(y);
            _setWidth(width);
            _setHeight(height);
        }
    }
}
