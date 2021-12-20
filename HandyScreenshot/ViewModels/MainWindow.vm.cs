using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HandyScreenshot.Helpers;
using HandyScreenshot.Mvvm;

namespace HandyScreenshot.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private static readonly byte[] SampleBytes = new byte[4];

        private string _dpiString = string.Empty;
        private bool _isToolBarActivated;

        public event EventHandler? CloseCommandInvoked;
        public event EventHandler? SaveCommandInvoked;

        public string DpiString
        {
            get => _dpiString;
            set => SetProperty(ref _dpiString, value);
        }

        public Func<int, int, Color> ColorGetter { get; }

        public BitmapSource Background { get; }

        public MonitorInfo MonitorInfo { get; }

        public ICommand CloseCommand { get; }

        public ICommand SaveCommand { get; }

        public ScreenshotState State { get; }

        public bool IsToolBarActivated
        {
            get => _isToolBarActivated;
            set => SetProperty(ref _isToolBarActivated, value, v => State.IsActivated = !v);
        }

        public MainWindowViewModel(ScreenshotState state, BitmapSource background, MonitorInfo monitorInfo)
        {
            State = state;
            Background = background;
            MonitorInfo = monitorInfo;
            ColorGetter = GetColorByCoordinate;

            CloseCommand = new RelayCommand(OnCloseCommandInvoked);
            SaveCommand = new RelayCommand(OnSaveCommandInvoked);
        }

        public void Initialize()
        {
            State.IsActivated = true;
            var initPoint = Win32Helper.GetPhysicalMousePosition();
            State.PushState(MouseMessage.MouseMove, initPoint.X, initPoint.Y);
        }

        private Color GetColorByCoordinate(int x, int y)
        {
            if (x < 0 || x >= Background.PixelWidth ||
                y < 0 || y >= Background.PixelHeight) return Colors.Transparent;

            Background.CopyPixels(new Int32Rect(x, y, 1, 1), SampleBytes, 4, 0);

            return Color.FromArgb(SampleBytes[3], SampleBytes[2], SampleBytes[1], SampleBytes[0]);
        }

        protected virtual void OnCloseCommandInvoked()
        {
            CloseCommandInvoked?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSaveCommandInvoked()
        {
            SaveCommandInvoked?.Invoke(this, EventArgs.Empty);
        }
    }
}
