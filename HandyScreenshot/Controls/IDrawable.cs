using System.Windows.Media;

namespace HandyScreenshot.Controls
{
    public interface IDrawable<T>
    {
        bool CanDraw(in T drawingData);

        void Draw(DrawingContext dc, in T drawingData);

        void UpdateScale(double scale);
    }
}
