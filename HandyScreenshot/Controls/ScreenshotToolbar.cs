using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HandyScreenshot.Common;
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

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            "IsActive", typeof(bool), typeof(ScreenshotToolbar),
            new PropertyMetadata(default(bool)));

        private static void OnScreenshotRectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.UpdateDependencyProperty<ScreenshotToolbar, ScreenshotState>(e,
                (self, newValue) =>
                {
                    newValue.ScreenshotRect.RectChanged += self.OnRectChanged;
                    newValue.PropertyChanged += self.OnStateChanged;
                },
                (self, oldValue) =>
                {
                    oldValue.ScreenshotRect.RectChanged -= self.OnRectChanged;
                    oldValue.PropertyChanged -= self.OnStateChanged;
                });
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

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        private FrameworkElement? _toolbar;

        private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.PropertyName == nameof(ScreenshotState.Mode))
                {
                    Visibility = ScreenshotState.Mode == ScreenshotMode.Fixed
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            });
        }

        private void OnRectChanged(int x, int y, int w, int h)
        {
            Dispatcher.Invoke(OnRectChangedInternal);
        }

        private void OnRectChangedInternal()
        {
            if (_toolbar == null || MonitorInfo == null) return;

            var rect = ScreenshotState.ScreenshotRect;
            var intersectedRect = MonitorInfo.PhysicalScreenRect.Intersect((rect.X, rect.Y, rect.Width, rect.Height));
            if (2 * intersectedRect.Width * intersectedRect.Height < rect.Width * rect.Height)
            {
                _toolbar.Visibility = Visibility.Collapsed;
                return;
            }

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

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            SetCurrentValue(IsActiveProperty, true);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            SetCurrentValue(IsActiveProperty, false);
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
