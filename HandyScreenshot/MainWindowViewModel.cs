using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using HandyScreenshot.Interop;
using HandyScreenshot.UIAInterop;

namespace HandyScreenshot
{
    public class MainWindowViewModel : BindableBase
    {
        private Point _mousePosition;
        private CachedElement _selectedElement;
        private Rect _rect;

        public CachedElement SelectedElement
        {
            get => _selectedElement;
            set => SetProperty(ref _selectedElement, value);
        }

        public Point MousePosition
        {
            get => _mousePosition;
            set => SetProperty(ref _mousePosition, value);
        }

        public Rect Rect
        {
            get => _rect;
            set => SetProperty(ref _rect, value);
        }

        public MonitorInfo MonitorInfo { get; set; }

        public ICommand CloseCommand { get; } = new RelayCommand(() => Application.Current.Shutdown());

        public MainWindowViewModel()
        {
            IReadOnlyList<CachedElement> elements = CachedElement.GetChildren(AutomationElement.RootElement, MonitorHelper.ScaleFactor);
            App.HookDisposables.Add(Win32Helper.HookMouseMoveEvent(point =>
            {
                MousePosition = point.ToPoint(MonitorHelper.ScaleFactor);
                if (MonitorInfo.ScreenRect.Contains(MousePosition))
                {
                    SelectedElement = GetAdjustElement(elements, MousePosition);
                    Rect = SelectedElement != null
                        ? RebaseRect(SelectedElement.Rect, MonitorInfo.ScreenRect.Left, MonitorInfo.ScreenRect.Top)
                        : Rect.Empty;
                }
                else
                {
                    SelectedElement = null;
                    Rect = Rect.Empty;
                }
            }));
        }

        private static CachedElement GetAdjustElement(IReadOnlyCollection<CachedElement> elements, Point point)
        {
            CachedElement result = null;

            while (true)
            {
                var temp = elements
                    .FirstOrDefault(item => item.Rect.Contains(point));

                if (temp == null) break;

                result = temp;
                var children = result.Children;
                if (children.Count > 0)
                {
                    elements = children;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private static Rect RebaseRect(Rect rect, double originX, double originY)
        {
            return new Rect(
                rect.Left - originX,
                rect.Top - originY,
                rect.Width,
                rect.Height);
        }
    }
}
