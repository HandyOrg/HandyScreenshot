using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using HandyScreenshot.Helpers;

namespace HandyScreenshot.Controls
{
    public class Magnifier : DrawingControlBase
    {
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale", typeof(double), typeof(Magnifier), new PropertyMetadata(default(double), OnScaleChanged));
        public static readonly DependencyProperty MousePositionProperty = DependencyProperty.Register(
            "MousePosition", typeof(PointProxy), typeof(Magnifier), new PropertyMetadata(default(PointProxy), OnMousePositionChanged));
        public static readonly DependencyProperty MagnifiedTargetProperty = DependencyProperty.Register(
            "MagnifiedTarget", typeof(Visual), typeof(Magnifier), new PropertyMetadata(default(Visual), OnMagnifiedTargetChanged));
        public static readonly DependencyProperty ColorGetterProperty = DependencyProperty.Register(
            "ColorGetter", typeof(Func<double, double, Color>), typeof(Magnifier), new PropertyMetadata(default(Func<double, double, Color>)));
        public static readonly DependencyProperty ClipBoxRectProperty = DependencyProperty.Register(
            "ClipBoxRect", typeof(RectProxy), typeof(Magnifier), new PropertyMetadata(default(RectProxy)));
        public static readonly DependencyProperty MonitorInfoProperty = DependencyProperty.Register(
            "MonitorInfo", typeof(MonitorInfo), typeof(Magnifier), new PropertyMetadata(default(MonitorInfo)));

        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<Magnifier, double>(e,
                (self, newValue) => self.UpdateScale(newValue));
        }

        private static void OnMagnifiedTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<Magnifier, Visual>(e,
                (self, newValue) => self._magnifiedRegionBrush.Visual = newValue);
        }

        private static void OnMousePositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<Magnifier, PointProxy>(e,
                (self, newValue) => newValue.PointChanged += self.OnMousePositionChanged,
                (self, oldValue) => oldValue.PointChanged -= self.OnMousePositionChanged);
        }

        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public PointProxy MousePosition
        {
            get => (PointProxy)GetValue(MousePositionProperty);
            set => SetValue(MousePositionProperty, value);
        }

        public Visual MagnifiedTarget
        {
            get => (Visual)GetValue(MagnifiedTargetProperty);
            set => SetValue(MagnifiedTargetProperty, value);
        }

        public Func<double, double, Color> ColorGetter
        {
            get => (Func<double, double, Color>)GetValue(ColorGetterProperty);
            set => SetValue(ColorGetterProperty, value);
        }

        public RectProxy ClipBoxRect
        {
            get => (RectProxy)GetValue(ClipBoxRectProperty);
            set => SetValue(ClipBoxRectProperty, value);
        }

        public MonitorInfo MonitorInfo
        {
            get => (MonitorInfo)GetValue(MonitorInfoProperty);
            set => SetValue(MonitorInfoProperty, value);
        }

        #region Constants: Magnifier Drawing Data

        private const int OffsetFromMouse = 20;
        private const int TargetRegionWidth = 19;
        private const int TargetRegionHeight = 13;
        private const int HalfTargetRegionWidth = TargetRegionWidth / 2;
        private const int HalfTargetRegionHeight = TargetRegionHeight / 2;
        private const int OnePixelMagnified = 8;
        private const int HalfMagnifiedOnePixelSize = OnePixelMagnified / 2;

        private const int MagnifierWidth = TargetRegionWidth * OnePixelMagnified;
        private const int MagnifierHeight = TargetRegionHeight * OnePixelMagnified;

        #endregion

        private static readonly Brush CrossLineBrush;
        private static readonly Brush InfoBackgroundBrush;

        static Magnifier()
        {
            CrossLineBrush = new SolidColorBrush(Color.FromArgb(0x60, 0x20, 0x80, 0xf0));
            CrossLineBrush.Freeze();
            InfoBackgroundBrush = new SolidColorBrush(Color.FromArgb(0xE0, 0, 0, 0));
            InfoBackgroundBrush.Freeze();
        }

        private readonly Pen _whiteThinPen;
        private readonly Pen _blackThinPen;
        private readonly Pen _crossLinePen;
        private readonly VisualBrush _magnifiedRegionBrush;

        private double _offsetFromMouse = OffsetFromMouse;
        private double _targetRegionWidth = TargetRegionWidth;
        private double _targetRegionHeight = TargetRegionHeight;
        private double _halfTargetRegionWidth = HalfTargetRegionWidth;
        private double _halfTargetRegionHeight = HalfTargetRegionHeight;
        private double _halfPixelMagnified = HalfMagnifiedOnePixelSize;
        private double _magnifierWidth = MagnifierWidth;
        private double _magnifierHeight = MagnifierHeight;

        public Magnifier()
        {
            _whiteThinPen = new Pen(Brushes.White, 1);
            _blackThinPen = new Pen(Brushes.Black, 1);
            _crossLinePen = new Pen(CrossLineBrush, OnePixelMagnified);

            _magnifiedRegionBrush = new VisualBrush { ViewboxUnits = BrushMappingMode.Absolute };
        }

        private void UpdateScale(double scale)
        {
            _whiteThinPen.Thickness = scale;
            _blackThinPen.Thickness = scale;
            _crossLinePen.Thickness = OnePixelMagnified * scale;

            _offsetFromMouse = OffsetFromMouse * scale;
            _targetRegionWidth = TargetRegionWidth * scale;
            _targetRegionHeight = TargetRegionHeight * scale;
            _halfTargetRegionWidth = HalfTargetRegionWidth * scale;
            _halfTargetRegionHeight = HalfTargetRegionHeight * scale;
            _halfPixelMagnified = HalfMagnifiedOnePixelSize * scale;
            _magnifierWidth = MagnifierWidth * scale;
            _magnifierHeight = MagnifierHeight * scale;
        }

        private void OnMousePositionChanged(double x, double y)
        {
            Dispatcher.Invoke(RefreshMagnifier);
        }

        private void RefreshMagnifier()
        {
            _magnifiedRegionBrush.Viewbox = new Rect(
                MousePosition.X - _halfTargetRegionWidth,
                MousePosition.Y - _halfTargetRegionHeight,
                _targetRegionWidth,
                _targetRegionHeight);

            GetDrawingVisual().Using(DrawMagnifier);
        }

        private void DrawMagnifier(DrawingContext dc)
        {
            if (!ClipBoxRect.Contains(MousePosition.X, MousePosition.Y)) return;

            var x = MousePosition.X + _offsetFromMouse;
            var y = MousePosition.Y + _offsetFromMouse;
            var width = _magnifierWidth;
            var height = _magnifierHeight;

            // 1. and 2. Prepare drawing data and add guidelines.

            var guidelines = new GuidelineSet();
            var halfPixel = Scale / 2;

            // Draw magnified region box

            var magnifierRect = new Rect(x, y, width, height);
            var innerRect = new Rect(x - Scale, y - Scale, width + 2 * Scale, height + 2 * Scale);
            var outlineRect = new Rect(x - 2 * Scale, y - 2 * Scale, width + 4 * Scale, height + 4 * Scale);

            guidelines.GuidelinesX.Add(outlineRect.Left);
            guidelines.GuidelinesX.Add(outlineRect.Right);
            guidelines.GuidelinesY.Add(outlineRect.Top);
            guidelines.GuidelinesY.Add(outlineRect.Bottom);

            // Draw cross line * 4

            var centerLineX = (innerRect.Left + innerRect.Right) / 2;
            var centerLineY = (innerRect.Top + innerRect.Bottom) / 2;

            guidelines.GuidelinesX.Add(centerLineX + _halfPixelMagnified);
            guidelines.GuidelinesX.Add(centerLineX - _halfPixelMagnified);
            guidelines.GuidelinesY.Add(centerLineY + _halfPixelMagnified);
            guidelines.GuidelinesY.Add(centerLineY - _halfPixelMagnified);

            // Draw center point box

            var centerInnerRect = new Rect(
                centerLineX - _halfPixelMagnified + halfPixel - Scale,
                centerLineY - _halfPixelMagnified + halfPixel - Scale,
                2 * (_halfPixelMagnified - halfPixel) + 2 * Scale,
                2 * (_halfPixelMagnified - halfPixel) + 2 * Scale);
            var centerOutlineRect = new Rect(
                centerLineX - _halfPixelMagnified + halfPixel - 2 * Scale,
                centerLineY - _halfPixelMagnified + halfPixel - 2 * Scale,
                2 * (_halfPixelMagnified - halfPixel) + 4 * Scale,
                2 * (_halfPixelMagnified - halfPixel) + 4 * Scale);

            guidelines.GuidelinesX.Add(centerInnerRect.Left - halfPixel);
            guidelines.GuidelinesX.Add(centerInnerRect.Right + halfPixel);
            guidelines.GuidelinesY.Add(centerInnerRect.Top - halfPixel);
            guidelines.GuidelinesY.Add(centerInnerRect.Bottom + halfPixel);

            var infoBackgroundRect = new Rect(outlineRect.Left, outlineRect.Bottom, outlineRect.Width, 72 * Scale);
            var color = ColorGetter(MousePosition.X, MousePosition.Y);
            var colorText = GetText($"#{color.R:X2}{color.G:X2}{color.B:X2}", 1 / Scale);
            var positionText = GetText($"({MousePosition.X / Scale:0}, {MousePosition.Y / Scale:0})", 1 / Scale);

            var positionTextX = centerLineX - positionText.Width / 2;
            var positionTextY = outlineRect.Bottom + 12 * Scale;
            var colorBlockSize = 14 * Scale;
            var colorComponentWidth = colorBlockSize + 8 * Scale + colorText.Width;
            var colorX = centerLineX - colorComponentWidth / 2;
            var colorTextX = colorX + colorComponentWidth - colorText.Width;
            var colorY = positionTextY + positionText.Height + 12 * Scale;
            var colorTextY = colorY + (colorBlockSize - colorText.Height) / 2;
            var positionTextPoint = new Point(positionTextX, positionTextY);
            var colorTextPoint = new Point(colorTextX, colorTextY);
            var colorBlockRect = new Rect(colorX, colorY, colorBlockSize, colorBlockSize);

            guidelines.GuidelinesX.Add(infoBackgroundRect.Bottom);
            guidelines.GuidelinesX.Add(colorBlockRect.Left - halfPixel);
            guidelines.GuidelinesX.Add(colorBlockRect.Right + halfPixel);
            guidelines.GuidelinesY.Add(colorBlockRect.Top - halfPixel);
            guidelines.GuidelinesY.Add(colorBlockRect.Bottom + halfPixel);

            // 3. Start Drawing

            dc.PushGuidelineSet(guidelines);

            // Draw magnified region box

            dc.DrawRectangle(Brushes.Black, null, outlineRect);
            dc.DrawRectangle(Brushes.White, null, innerRect);
            dc.DrawRectangle(_magnifiedRegionBrush, null, magnifierRect);

            // Draw cross line * 4

            dc.DrawLine(
                _crossLinePen,
                new Point(centerLineX, innerRect.Top),
                new Point(centerLineX, centerLineY - _halfPixelMagnified - 2 * Scale));
            dc.DrawLine(
                _crossLinePen,
                new Point(centerLineX, innerRect.Bottom),
                new Point(centerLineX, centerLineY + _halfPixelMagnified + 2 * Scale));
            dc.DrawLine(
                _crossLinePen,
                new Point(innerRect.Left, centerLineY),
                new Point(centerLineX - _halfPixelMagnified - 2 * Scale, centerLineY));
            dc.DrawLine(
                _crossLinePen,
                new Point(innerRect.Right, centerLineY),
                new Point(centerLineX + _halfPixelMagnified + 2 * Scale, centerLineY));

            // Draw center point box

            dc.DrawRectangle(Brushes.Transparent, _blackThinPen, centerOutlineRect);
            dc.DrawRectangle(Brushes.Transparent, _whiteThinPen, centerInnerRect);

            // Draw info board

            dc.DrawRectangle(InfoBackgroundBrush, null, infoBackgroundRect);
            dc.DrawText(positionText, positionTextPoint);
            dc.DrawRectangle(new SolidColorBrush(color), _whiteThinPen, colorBlockRect);
            dc.DrawText(colorText, colorTextPoint);

            dc.Pop();
        }

        private static FormattedText GetText(string text, double pixelsPerDip)
        {
            return new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Consolas"),
                12,
                Brushes.White,
                pixelsPerDip);
        }
    }
}
