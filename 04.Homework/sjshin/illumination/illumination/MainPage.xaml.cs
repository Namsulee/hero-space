using System;
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
using Windows.Devices.I2c;
using Windows.Devices.Enumeration;
using Windows.System.Diagnostics;
using Windows.System.Threading;
using System.Threading;
using System.Diagnostics;


// 빈 페이지 항목 템플릿은 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 에 문서화되어 있습니다.

namespace illumination
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    /// 
    

    public sealed partial class MainPage : Page
    {

        private I2cDevice GY30;
        private Timer periodicTimer;
        public MainPage()
        {
            this.InitializeComponent();
            InitDY30();

        }
        private async void InitDY30()
        {
            try
            {
                string aqs = I2cDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);
                var settings = new I2cConnectionSettings(0x23);
                settings.BusSpeed = I2cBusSpeed.FastMode;
                GY30 = await I2cDevice.FromIdAsync(dis[0].Id, settings);

                if (GY30 == null)
                {
                    textBox.Text = string.Format(
                    "Slave address {0} on I2C Controller {1} is currently in use by " +
                    "another application. Please ensure that no other applications are using I2C.",
                    settings.SlaveAddress,
                    dis[0].Id);
                    return;
                }
                // GY30.Write(new byte[] { 0x01 });
                // GY30.Write(new byte[] { 0x00 });

            }
            catch (Exception error)
            {
                Debug.WriteLine("Exception: " + error.Message);
            }
            //GY30.ConnectionSettings.
            periodicTimer = new Timer(TimerCallback, null, 0, 100);
        }
        private void TimerCallback(object state)
        {
            string luxText;
            var lux = ReadI2CLux();
            // int jj;
            Debug.WriteLine("LUX : " + lux);
            luxText = String.Format("LUX: {0} value", lux);

            // if (lux < 200) OnLed();
            //  else OffLed();

            var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                textBox.Text = luxText;
            });

        }
        private int ReadI2CLux()
        {
            byte[] regAddrBuf = new byte[] { 0x23 };
            byte[] readBuf = new byte[2];

            GY30.WriteRead(regAddrBuf, readBuf);

            // is this calculation correct?
            var valf = ((readBuf[0] << 8) | readBuf[1]) / 1.2;
            // double luxVal = (vall + (256 * valh)) / 1.2;

            return (int)valf;
        }


    }
}
