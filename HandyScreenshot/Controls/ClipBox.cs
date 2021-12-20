﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using HandyScreenshot.Common;
using HandyScreenshot.Helpers;

namespace HandyScreenshot.Controls
{
    public class ClipBox : FrameworkElement
    {
        public static readonly DependencyProperty RectProxyProperty = DependencyProperty.Register(
            "RectProxy", typeof(RectProxy), typeof(ClipBox), new PropertyMetadata(null));
        public static readonly DependencyProperty MonitorInfoProperty = DependencyProperty.Register(
            "MonitorInfo", typeof(MonitorInfo), typeof(ClipBox), new PropertyMetadata(default(MonitorInfo)));

        public RectProxy RectProxy
        {
            get => (RectProxy)GetValue(RectProxyProperty);
            set => SetValue(RectProxyProperty, value);
        }

        public MonitorInfo MonitorInfo
        {
            get => (MonitorInfo)GetValue(MonitorInfoProperty);
            set => SetValue(MonitorInfoProperty, value);
        }

        public Visual Visual { get; }

        private readonly VisualCollection _visualCollection;

        public ClipBox()
        {
            Visual = new ClipBoxVisual();
            BindingOperations.SetBinding(Visual,
                ClipBoxVisual.RectProxyProperty,
                new Binding(nameof(RectProxy)) { Source = this });
            BindingOperations.SetBinding(Visual,
                ClipBoxVisual.MonitorInfoProperty,
                new Binding(nameof(ClipBoxVisual.MonitorInfo)) { Source = this });

            var clipBoxPointVisual = new ClipBoxPointVisual();
            BindingOperations.SetBinding(clipBoxPointVisual,
                ClipBoxPointVisual.RectProxyProperty,
                new Binding(nameof(RectProxy)) { Source = this });
            BindingOperations.SetBinding(clipBoxPointVisual,
                ClipBoxPointVisual.MonitorInfoProperty,
                new Binding(nameof(ClipBoxPointVisual.MonitorInfo)) { Source = this });

            _visualCollection = new VisualCollection(this)
            {
                Visual,
                clipBoxPointVisual
            };
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var visual in _visualCollection)
            {
                if (visual is FrameworkElement frameworkElement)
                {
                    frameworkElement.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
                }
            }

            return base.ArrangeOverride(finalSize);
        }

        protected override int VisualChildrenCount => _visualCollection.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _visualCollection.Count)
            {
                throw new IndexOutOfRangeException();
            }

            return _visualCollection[index];
        }

        #region Internal Members

        internal const int PrimaryPenThickness = 2;
        internal static readonly Brush PrimaryBrush;
        internal static readonly Brush MaskBrush;
        internal static readonly Pen WhitePen;

        static ClipBox()
        {
            PrimaryBrush = new SolidColorBrush(Color.FromRgb(0x20, 0x80, 0xf0));
            PrimaryBrush.Freeze();
            MaskBrush = new SolidColorBrush(Color.FromArgb(0xA0, 0, 0, 0));
            MaskBrush.Freeze();
            WhitePen = new Pen(Brushes.White, 1.5);
            WhitePen.Freeze();
        }

        internal static Pen CreatePrimaryPen(double scale)
        {
            var result = new Pen(PrimaryBrush, PrimaryPenThickness * scale);
            result.Freeze();
            return result;
        }

        #endregion
    }
}
