using System.Windows.Media;

namespace HandyScreenshot.Controls
{
    public interface IDrawable<in T>
    {
        bool CanDraw(T drawingData);

        void Draw(DrawingContext dc, T drawingData);
    }
}
