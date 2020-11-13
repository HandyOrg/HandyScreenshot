using System;
using System.Windows;
using System.Windows.Media;
using HandyScreenshot.Helpers;

namespace HandyScreenshot.Controls
{
    internal class ClipBoxVisual : DrawingControlBase
    {
        private const int BackgroundIndex = 0;
        private const int ClipBoxIndex = 1;

        public static readonly DependencyProperty RectProxyProperty = DependencyProperty.Register(
            "RectProxy", typeof(RectProxy), typeof(ClipBoxVisual), new PropertyMetadata(default(RectProxy), OnRectProxyChanged));
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background", typeof(ImageSource), typeof(ClipBoxVisual), new PropertyMetadata(default(ImageSource), OnBackgroundChanged));

        private static void OnRectProxyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<ClipBoxVisual, RectProxy>(e,
                (self, newValue) => newValue.RectChanged += self.OnRectChanged,
                (self, oldValue) => oldValue.RectChanged -= self.OnRectChanged);
        }

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<ClipBoxVisual, ImageSource>(e,
                (self, newValue) => self.RefreshBackground());
        }

        public RectProxy RectProxy
        {
            get => (RectProxy)GetValue(RectProxyProperty);
            set => SetValue(RectProxyProperty, value);
        }

        public ImageSource Background
        {
            get => (ImageSource)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        private readonly Brush MaskBrush;
        private readonly Pen PrimaryPen;

        public ClipBoxVisual() : base(2)
        {
            MaskBrush = new SolidColorBrush(Color.FromArgb(0xA0, 0, 0, 0));
            MaskBrush.Freeze();
            Brush primaryBrush = new SolidColorBrush(Color.FromRgb(0x20, 0x80, 0xf0));
            primaryBrush.Freeze();
            PrimaryPen = new Pen(primaryBrush, 1.6);
            PrimaryPen.Freeze();
        }

        private void OnRectChanged(double x, double y, double w, double h)
        {
            Dispatcher.Invoke(RefreshClipBox);
        }

        // ReSharper disable once RedundantArgumentDefaultValue
        private void RefreshBackground() => GetDrawingVisual(BackgroundIndex).Using(DrawBackground);

        private void RefreshClipBox() => GetDrawingVisual(ClipBoxIndex).Using(DrawClipBox);

        private void DrawBackground(DrawingContext dc)
        {
            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.NearestNeighbor);
            var groupDc = group.Open();
            groupDc.DrawImage(Background, new Rect(0, 0, ActualWidth, ActualHeight));
            groupDc.Close();
            dc.DrawDrawing(group);
        }

        private void DrawClipBox(DrawingContext dc)
        {
            var halfPenThickness = PrimaryPen.Thickness / 2;

            var x = RectProxy.X - halfPenThickness;
            var y = RectProxy.Y - halfPenThickness;
            var w = RectProxy.Width + PrimaryPen.Thickness + halfPenThickness;
            var h = RectProxy.Height + PrimaryPen.Thickness + halfPenThickness;

            var x0 = Math.Max(x, 0);
            var y0 = Math.Max(y, 0);
            var w0 = Math.Max(w, 0);
            var h0 = Math.Max(h, 0);

            var r = x + w;
            var b = y + h;

            var leftRect = new Rect(0, y, x0, h0);
            var topRect = new Rect(0, 0, ActualWidth, y0);
            var rightRect = new Rect(r, y, Math.Max(ActualWidth - r, 0), h0);
            var bottomRect = new Rect(0, b, ActualWidth, Math.Max(ActualHeight - b, 0));
            var centralRect = new Rect(x, y, w0, h0);

            var guidelines = new GuidelineSet(
                new[] { centralRect.Left + halfPenThickness, centralRect.Right - halfPenThickness },
                new[] { centralRect.Top + halfPenThickness, centralRect.Bottom - halfPenThickness });

            dc.PushGuidelineSet(guidelines);

            dc.DrawRectangle(MaskBrush, null, leftRect);
            dc.DrawRectangle(MaskBrush, null, topRect);
            dc.DrawRectangle(MaskBrush, null, rightRect);
            dc.DrawRectangle(MaskBrush, null, bottomRect);
            dc.DrawRectangle(Brushes.Transparent, PrimaryPen, centralRect);

            dc.Pop();
        }
    }
}
