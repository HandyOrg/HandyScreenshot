using System.Windows;
using System.Windows.Media;

namespace HandyScreenshot.Controls
{
    internal class DrawingBackground : IDrawable<(ImageSource background, double width, double height)>
    {
        public bool CanDraw((ImageSource background, double width, double height) drawingData) => true;

        public void Draw(DrawingContext dc, (ImageSource background, double width, double height) drawingData)
        {
            var (background, width, height) = drawingData;
            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.NearestNeighbor);
            var groupDc = group.Open();
            groupDc.DrawImage(background, new Rect(0, 0, width, height));
            groupDc.Close();
            dc.DrawDrawing(group);
        }
    }
}
