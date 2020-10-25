using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace HandyScreenshot.Controls
{
    [TemplatePart(Name = ImageName, Type = typeof(Image))]
    public class MosaicDrawingBoard : Control
    {
        private const string ImageName = "PART_Image";

        private Image _image;
        private WriteableBitmap _wb;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _image = (Image)GetTemplateChild(ImageName);
            //_image.Source = _wb = new WriteableBitmap(); // TODO

            _image.MouseLeftButtonDown += ImageOnMouseLeftButtonDown;
            _image.MouseMove += ImageOnMouseMove;
            _image.MouseLeftButtonUp += ImageOnMouseLeftButtonUp;
            _image.MouseLeave += ImageOnMouseLeave;
        }

        private void ImageOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ImageOnMouseMove(object sender, MouseEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ImageOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ImageOnMouseLeave(object sender, MouseEventArgs e)
        {
            throw new NotImplementedException();
        }

        static MosaicDrawingBoard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MosaicDrawingBoard), new FrameworkPropertyMetadata(typeof(MosaicDrawingBoard)));
        }
    }
}
