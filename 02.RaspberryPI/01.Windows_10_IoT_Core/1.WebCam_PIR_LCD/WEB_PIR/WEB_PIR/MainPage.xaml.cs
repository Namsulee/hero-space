using System;                      
using System.Diagnostics;                   // Debug 용
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Windows.Media.Capture;                // 동영상 촬영용
using Windows.Media.MediaProperties;        // 동영상 속성 설정용
using System.Threading;                     // 타이머용
using System.Threading.Tasks;               // 타이머용
using Windows.Storage;                      // 동영상 파일명
using Windows.Devices.Gpio;                 // GPIO용
using Windows.Devices.Enumeration;
     

// Add Bongchae 2016.10.15
// Using GitHub

namespace WEB_PIR
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture capture;                       // 캡쳐 객체
        private StorageFile videoFile;                      // 동영상 저장용
        private string fileName = "video.mp4";        // 저장용 파일 이름
        private const string storageName = "[StorageName]"; // 업로드할 곳의 저장소 이름
        private const string storageKey = "[StorageKey]";   // 업로드할 곳의 접근 Key
        private Timer timer;                // 녹화용 타이머
        private bool isRecording = false;   // 녹화용 플래그
        private bool isSensor = false;      // 센서용 플래그
        private const int inputPin = 5;    // 센서 입력용 GPIO 핀
        private const int actPin = 12;      // 동작 확인용 ACTLED
        private GpioPin pin;
        private GpioPin statusPin;
        LCD lcd;

        public MainPage()
        {
            this.InitializeComponent();
            initLCD();
            Loaded += MainPage_Loaded;
        }

        private async void initLCD()
        {
            lcd = new LCD(16, 2);
            await lcd.InitAsync(20, 16, 2, 3, 4, 17); //16X2 LCD and works on 4bits
            await lcd.clearAsync();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await initWebcam();
            await initGpio();
        }

        private async Task initWebcam()
        {
            Debug.WriteLine("initializing Web Camera...");

            try
            {
                if (capture != null)
                {
                    if (isRecording)
                    {
                        await capture.StopRecordAsync();
                        isRecording = false;
                    }

                    capture.Dispose();
                    capture = null;
                }
                var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

                if (devices.Count == 0)
                {
                    Debug.WriteLine("Not able to find Webcam.");
                    return;
                }

                capture = new MediaCapture();
                await capture.InitializeAsync();

                await Task.Delay(10000);

                Debug.WriteLine("Initialized Webcam");
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task initGpio()
        {
            var gpio = GpioController.GetDefault();
            if (gpio == null)
            {
                Debug.WriteLine("Not found GPIO");
                return;
            }

            // Set GPIO
            pin = gpio.OpenPin(inputPin);

            if (pin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            {
                pin.SetDriveMode(GpioPinDriveMode.Input);
            }
            else
            {
                pin.SetDriveMode(GpioPinDriveMode.Input);
            }

            // ACT LED 설정
            statusPin = gpio.OpenPin(actPin);
            statusPin.Write(GpioPinValue.High);
            statusPin.SetDriveMode(GpioPinDriveMode.Output);

            Debug.WriteLine("Initiaizing GPIO");

            // wait
            await Task.Delay(10000);

            // 이벤트 등록
            pin.ValueChanged += Pin_ValueChanged;

            Debug.WriteLine("Initialized GPIO");
            statusPin.Write(GpioPinValue.Low); //If all devices are reday, LED should be turned on
        }

        private void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                Debug.WriteLine("Rise");
                lcd.WriteLine(" ");
                lcd.WriteLine("Detected");
                if (isRecording == false && isSensor == false)
                {
                    startRecording();
                }
            }

            if(args.Edge == GpioPinEdge.FallingEdge)
            {
                Debug.WriteLine("Fall");
                lcd.WriteLine(" ");
                lcd.WriteLine("Clear");
                if (isSensor == true)
                {
                    isSensor = false;
                }
            }
        }

        private async void startRecording()
        {
            if (isSensor == false)
            {
                //Set filename accoring to current time
                string time = DateTime.Now.ToString("yyyyMMddHHMMss");
                fileName = string.Format("capture_{0}.mp4", time);
                //Setup Recording File
                videoFile = await Windows.Storage.KnownFolders.VideosLibrary.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);
                MediaEncodingProfile recordProfile = null;
                recordProfile = MediaEncodingProfile.CreateMp4(Windows.Media.MediaProperties.VideoEncodingQuality.Auto);
                Debug.WriteLine("Video storage file preparation successful");
                await capture.StartRecordToStorageFileAsync(recordProfile, videoFile);

                isRecording = true;
                isSensor = true;
                Debug.WriteLine("Start Recording");

                var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    status.Text = "Start Recording";

                });

                timer = new Timer(TimerCallback, null, 5000, 5000);
            }
        }
        private async void PlayVideo()
        {
            Debug.WriteLine("Play Video");
            if (videoFile.Path != null)
            {
               // StorageFile playFile = videoFile;
                var stream = await videoFile.OpenReadAsync();
                playbackElement.AutoPlay = true;
                playbackElement.SetSource(stream, videoFile.FileType);
                playbackElement.Play();
            }
        }
        private async void TimerCallback(object state)
        {
            try
            {
                Debug.WriteLine("TimerCallback");

                if (isRecording == true)
                {
                    timer.Dispose();  //Timer 클래스에서 사용중인 모든 리소스를 해지하고 다시 사용할 수 있는 상태로 바뀜
                    await capture.StopRecordAsync();

                    Debug.WriteLine("Stop Recording");
                    Debug.WriteLine("path : " + videoFile.Path);

              
                    isRecording = false;
                    //await fileUpload();
                    //Debug.WriteLine("Upload Complete");
                    var task = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                       status.Text = "Recorded file and Play";
                       PlayVideo();
                    });
                }
            }
            catch(Exception ex)
            {
                
                if (ex is System.UnauthorizedAccessException)
                {
                    Debug.WriteLine("Unable to play recorded video; video recorded successfully to: " + videoFile.Path);

                }
                else
                {
                    Debug.WriteLine(ex.Message);

                }
               
            }
            
        }
    }
}
