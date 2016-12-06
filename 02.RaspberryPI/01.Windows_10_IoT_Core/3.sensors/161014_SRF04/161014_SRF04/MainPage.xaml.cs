using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Gpio;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace _161014_SRF04
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        LCD lcd;
        private double distance;
        SRF04 ultraSonicDistanceDetector;
        private const int GPIO_TRIGGER = 23;
        private const int GPIO_ECHO = 24;
        public MainPage()
        {
            this.InitializeComponent();
            initGPIO();
            initSensor();
            initLCD();
            loopSensor();
        }
        private void initSensor()
        {
            ultraSonicDistanceDetector = new SRF04(GPIO_TRIGGER, GPIO_ECHO);
        }
        private void initGPIO()
        {
            Debug.WriteLine("Init GPIO");
            var gpio = GpioController.GetDefault();
            if (gpio == null)
            {
                GpioStatus.Text = "There is no GPIO controller on this device";
                return;
            }
            else
            {
                GpioStatus.Text = "GPIO initialization completed";
            }
        }
        private async void initLCD()
        {
            lcd = new LCD(16, 2);
            await lcd.InitAsync(20, 16, 2, 3, 4, 17);
            await lcd.clearAsync();
        }
        private async void loopSensor()
        {
            await lcd.clearAsync();
            lcd.write("U.S. Ranger");
            while (true)
            {
                distance = Math.Round(ultraSonicDistanceDetector.GetDistance());
                await Task.Delay(1000);
                Debug.WriteLine("Distance : " + distance + "cm");
                lcd.setCursor(0, 1);
                lcd.WriteLine("Distance : " + distance + "cm"); var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    DistanceText.Text = "Disatnace : " + distance.ToString() + "cm";
                });
            }
        }
    }
}