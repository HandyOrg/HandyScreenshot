namespace HandyScreenshot.Common
{
    public delegate void PointChangedEventHandler(double x, double y);

    public class PointProxy
    {
        public event PointChangedEventHandler PointChanged;

        public double X { get; private set; }

        public double Y { get; private set; }

        public void Set(double x, double y)
        {
            X = x;
            Y = y;
            OnPointChanged();
        }

        protected virtual void OnPointChanged() => PointChanged?.Invoke(X, Y);
    }
}
