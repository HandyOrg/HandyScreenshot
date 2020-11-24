using System;
using System.Windows;
using System.Windows.Controls;
using HandyScreenshot.Helpers;
using HandyScreenshot.ViewModels;

namespace HandyScreenshot.Controls
{
    [TemplatePart(Name = ToolbarName, Type = typeof(FrameworkElement))]
    public class ScreenshotToolbar : Control
    {
        private const string ToolbarName = "PART_Toolbar";
        private const double OffsetY = 8;


        public static readonly DependencyProperty ScreenshotStateProperty = DependencyProperty.Register(
            "ScreenshotState", typeof(ScreenshotState), typeof(ScreenshotToolbar),
            new PropertyMetadata(default(ScreenshotState), OnScreenshotRectChanged));

        public static readonly DependencyProperty MonitorInfoProperty = DependencyProperty.Register(
            "MonitorInfo", typeof(MonitorInfo), typeof(ScreenshotToolbar),
            new PropertyMetadata(default(MonitorInfo)));

        private static void OnScreenshotRectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<ScreenshotToolbar, ScreenshotState>(e,
                (self, newValue) => newValue.ScreenshotRect.RectChanged += self.OnRectChanged,
                (self, oldValue) => oldValue.ScreenshotRect.RectChanged -= self.OnRectChanged);
        }


        public ScreenshotState ScreenshotState
        {
            get => (ScreenshotState)GetValue(ScreenshotStateProperty);
            set => SetValue(ScreenshotStateProperty, value);
        }

        public MonitorInfo? MonitorInfo
        {
            get => (MonitorInfo?)GetValue(MonitorInfoProperty);
            set => SetValue(MonitorInfoProperty, value);
        }


        private FrameworkElement? _toolbar;

        public void OnRectChanged(int x, int y, int w, int h)
        {
            Dispatcher.Invoke(OnRectChangedInternal);
        }

        public void OnRectChangedInternal()
        {
            if (_toolbar == null || MonitorInfo == null) return;
            //if (ScreenshotState.Mode != ScreenshotMode.Fixed)
            //{
            //    _toolbar.Visibility = Visibility.Collapsed;
            //    return;
            //}

            //_toolbar.Visibility = Visibility.Visible;

            var rect = ScreenshotState.ScreenshotRect;
            var (wpfX, wpfY, wpfWidth, wpfHeight) = MonitorInfo.ToWpfAxis(
                rect.X, rect.Y, rect.Width, rect.Height);

            var left = wpfX + wpfWidth - _toolbar.ActualWidth;
            left = Math.Max(left, 0);
            left = Math.Min(left, MonitorInfo.WpfAxisScreenRect.Width - _toolbar.ActualWidth);

            var top = wpfY + wpfHeight + OffsetY;
            if (top + _toolbar.ActualHeight > MonitorInfo.WpfAxisScreenRect.Height)
            {
                top = wpfY - _toolbar.ActualHeight - OffsetY;
            }

            Canvas.SetLeft(_toolbar, left);
            Canvas.SetTop(_toolbar, top);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _toolbar = GetTemplateChild(ToolbarName) as FrameworkElement;
        }

        static ScreenshotToolbar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScreenshotToolbar), new FrameworkPropertyMetadata(typeof(ScreenshotToolbar)));
        }
    }
}
