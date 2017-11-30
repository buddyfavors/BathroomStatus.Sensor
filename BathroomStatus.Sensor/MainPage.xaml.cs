using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace BathroomStatus.Sensor
{
    public sealed partial class MainPage : Page
    {
        private const int DOOR_PIN = 5;

        private GpioController m_Gpio = null;
        private GpioPin m_DoorPin = null;
        private bool m_IsOpened = true;
        private readonly DispatcherTimer m_Timer = null;

        public MainPage()
        {
            InitializeComponent();

            InitGPIOAsync();

            Unloaded += MainPage_Unloaded;

            m_Timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            m_Timer.Tick += Timer_Tick;
            //m_Timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            m_DoorPin.Write(!IsOpened() ? GpioPinValue.Low : GpioPinValue.High);

            UpdateStatusAsync();
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            m_DoorPin.Dispose();
        }

        private async void InitGPIOAsync()
        {
            m_Gpio = await GpioController.GetDefaultAsync();
            
            m_DoorPin = m_Gpio.OpenPin(DOOR_PIN);
            m_DoorPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
            //m_DoorPin.Write(GpioPinValue.High);
            m_DoorPin.SetDriveMode(GpioPinDriveMode.Input);
            m_DoorPin.ValueChanged += DoorPin_ValueChanged;
        }

        private void DoorPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            m_IsOpened = !m_IsOpened;
            UpdateStatusAsync();
        }

        private bool IsOpened()
        {
            return m_IsOpened;
            var value = m_DoorPin.Read();
            return value == GpioPinValue.Low;
        }

        private async void UpdateStatusAsync()
        {
            var isOpened = IsOpened();

            await RunOnUIThreadAsync(CoreDispatcherPriority.High, () =>
            {
                var brush = new SolidColorBrush(isOpened ? Color.FromArgb(255, 0, 255, 0) : Color.FromArgb(255, 255, 0, 0));
                m_Overlay.Background = brush;
            });
        }

        private async Task RunOnUIThreadAsync(CoreDispatcherPriority priority, Action action)
        {
            await Dispatcher.RunAsync(priority, () => action());
        }
    }
}
