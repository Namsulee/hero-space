using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Devices.Gpio;                 // GPIO용
using Windows.Devices.I2c;
using Windows.Devices.Enumeration;

using CAMEARA_BASED_DB.ExtraDevice;      // User defined class
using CAMEARA_BASED_DB.Definition;       // User defined constants
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Windows.UI.Core;

// 빈 페이지 항목 템플릿은 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 에 문서화되어 있습니다.

namespace CAMEARA_BASED_DB
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StorageFile photoFile;
        private string fileName = "photo.jpeg";        // 저장용 파일 이름

        private bool isSensor = false;      // 센서용 플래그
        
        private GpioPin statusPin;
        LCD lcd;
        private WebCam webcam;
        FaceRectangle[] _faceRectangles;
        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient(OpenAPIConstants.OxfordFaceAPIKey);
        Guid _faceId;
        Guid[]  _facesId;
        private bool strangerDetected = false;
        //PIR Sensor
        private GpioPin pin_18;
        //IR LED
        private GpioPin pin_17;
        private GpioPin pin_27;
        private GpioPin pin_22;
        private GpioPin pin_10;
        private GpioPin pin_9;
        private GpioPin pin_11;
        private GpioPin pin_5;
        private GpioPin pin_6;
        private GpioPinValue pinValue;
        private I2cDevice GY30;
        ///private string _faceListName = string.Empty;

        // Whitelist Related Variables:
        private List<Visitor> whitelistedVisitors = new List<Visitor>();
        private StorageFolder whitelistFolder;
        private bool currentlyUpdatingWhitelist;

        // GUI Related Variables:
        private double visitorIDPhotoGridMaxWidth = 0;

        public MainPage()
        {
            this.InitializeComponent();
            // Causes this page to save its state when navigating to other pages
            NavigationCacheMode = NavigationCacheMode.Enabled;

            initializeDB();
            InitGpioIRLED();
            InitGY30();

            if (OpenAPIConstants.DisableLiveCameraFeed)
            {
                LiveFeedPanel.Visibility = Visibility.Collapsed;
                DisabledFeedGrid.Visibility = Visibility.Visible;
            }
            else
            {
                LiveFeedPanel.Visibility = Visibility.Visible;
                DisabledFeedGrid.Visibility = Visibility.Collapsed;
            }

        }


        public async void initializeDB()
        {
            UpdateWhitelistedVisitors();
            StorageFolder userList = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFolderAsync(OpenAPIConstants.WhiteListFolderName, CreationCollisionOption.OpenIfExists);
            var subfolders = await userList.GetFoldersAsync();
            var i = 0;
            _facesId = new Guid[OpenAPIConstants.MaxUserCnt];
            foreach (StorageFolder folder in subfolders)
            {
                Debug.WriteLine(folder.Name);

                var files = await folder.GetFilesAsync();
                foreach(var file in files)
                {
                    Debug.WriteLine("File Name :"+ file.Name);
                    try
                    {
                        var id = await DetectFaceFromImage(file);
                        _facesId[i++] = id;
                        Debug.WriteLine("This {0} ID to add into userlist successfully!", id);
                    }
                    catch(FaceAPIException ex)
                    {
                        Debug.WriteLine("Error Response{0}. {1]", ex.ErrorCode, ex.ErrorMessage);
                    }
                }
            }
        }

        private void InitGpioIRLED()
        {
            var gpio = GpioController.GetDefault();
            if (gpio == null)
            {
                return;
            }
            // LED PIN Initialize

            pin_17 = gpio.OpenPin(GpioConstants.LED_PIN_17);
            pin_27 = gpio.OpenPin(GpioConstants.LED_PIN_27);

            pin_22 = gpio.OpenPin(GpioConstants.LED_PIN_22);
            pin_10 = gpio.OpenPin(GpioConstants.LED_PIN_10);

            pin_9 = gpio.OpenPin(GpioConstants.LED_PIN_9);
            pin_11 = gpio.OpenPin(GpioConstants.LED_PIN_11);

            pin_5 = gpio.OpenPin(GpioConstants.LED_PIN_5);
            pin_6 = gpio.OpenPin(GpioConstants.LED_PIN_6);

            pinValue = GpioPinValue.Low;
            pin_17.Write(pinValue);
            pin_27.Write(pinValue);

            pin_22.Write(pinValue);
            pin_10.Write(pinValue);

            pin_9.Write(pinValue);
            pin_11.Write(pinValue);

            pin_5.Write(pinValue);
            pin_6.Write(pinValue);

            pin_17.SetDriveMode(GpioPinDriveMode.Output);
            pin_27.SetDriveMode(GpioPinDriveMode.Output);

            pin_22.SetDriveMode(GpioPinDriveMode.Output);
            pin_10.SetDriveMode(GpioPinDriveMode.Output);

            pin_9.SetDriveMode(GpioPinDriveMode.Output);
            pin_11.SetDriveMode(GpioPinDriveMode.Output);

            pin_5.SetDriveMode(GpioPinDriveMode.Output);
            pin_6.SetDriveMode(GpioPinDriveMode.Output);

            //pir sensor
            pin_18 = gpio.OpenPin(GpioConstants.PirSensorPin);

            if (pin_18.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            {
                pin_18.SetDriveMode(GpioPinDriveMode.Input);
            }
            else
            {
                pin_18.SetDriveMode(GpioPinDriveMode.Input);
            }

            Debug.WriteLine("Initiaizing GPIO");

            // wait
            //await Task.Delay(10000);

            // 이벤트 등록
            pin_18.ValueChanged += Pin_ValueChanged;

            Debug.WriteLine("Initialized GPIO");
        }

        private async void InitGY30()
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
                    Debug.WriteLine("Not able to ope GY30");
                    return;
                }
            }
            catch (Exception error)
            {
                Debug.WriteLine("Exception: " + error.Message);
            }
            //GY30.ConnectionSettings.
           // periodicTimer = new Timer(TimerCallback, null, 0, 100);
        }

        private int ReadI2CLux()
        {
            byte[] regAddrBuf = new byte[] { 0x23 };
            byte[] readBuf = new byte[2];

            GY30.WriteRead(regAddrBuf, readBuf);

            var valf = ((readBuf[0] << 8) | readBuf[1]) / 1.2;
            return (int)valf;
        }

        private void actionLed()
        {
            var lux = ReadI2CLux();
            Debug.WriteLine("LUX : " + lux);
            if (lux < 10)
            {
                Debug.WriteLine("IR ON");
                pin_17.Write(GpioPinValue.High);
                pin_27.Write(GpioPinValue.High);

                pin_22.Write(GpioPinValue.High);
                pin_10.Write(GpioPinValue.High);

                pin_9.Write(GpioPinValue.High);
                pin_11.Write(GpioPinValue.High);

                pin_5.Write(GpioPinValue.High);
                pin_6.Write(GpioPinValue.High);
            }
            else
            {
                Debug.WriteLine("IR OFF");
                pin_17.Write(GpioPinValue.Low);
                pin_27.Write(GpioPinValue.Low);

                pin_22.Write(GpioPinValue.Low);
                pin_10.Write(GpioPinValue.Low);

                pin_9.Write(GpioPinValue.Low);
                pin_11.Write(GpioPinValue.Low);

                pin_5.Write(GpioPinValue.Low);
                pin_6.Write(GpioPinValue.Low);
            }
        }
        private async Task<Guid> DetectFaceFromImage(StorageFile imageFile)
        {
            var stream = await imageFile.OpenStreamForReadAsync();
            var faces = await faceServiceClient.DetectAsync(stream);
            if (faces == null || faces.Length < 1)
            {
               // throw new FaceRecognitionException(FaceRecognitionExceptionType.NoFaceDetected);
            }
            else if (faces.Length > 1)
            {
                //throw new FaceRecognitionException(FaceRecognitionExceptionType.MultipleFacesDetected);
            }

            return faces[0].FaceId;
        }

        private async void WebcamFeed_Loaded(object sender, RoutedEventArgs e)
        {
            if (webcam == null || !webcam.IsInitialized())
            {
                // Initialize Webcam Helper
                webcam = new WebCam();
                await webcam.InitializeCameraAsync();

                // Set source of WebcamFeed on MainPage.xaml
                WebcamFeed.Source = webcam.mediaCapture;

                // Check to make sure MediaCapture isn't null before attempting to start preview. Will be null if no camera is attached.
                if (WebcamFeed.Source != null)
                {
                    // Start the live feed
                    LiveFeedPanel.Visibility = Visibility.Visible;
                    DisabledFeedGrid.Visibility = Visibility.Collapsed;
                    await webcam.StartCameraPreview();
                }
            }
            else if (webcam.IsInitialized())
            {
                WebcamFeed.Source = webcam.mediaCapture;

                // Check to make sure MediaCapture isn't null before attempting to start preview. Will be null if no camera is attached.
                if (WebcamFeed.Source != null)
                {
                    LiveFeedPanel.Visibility = Visibility.Visible;
                    DisabledFeedGrid.Visibility = Visibility.Collapsed;

                    await webcam.StartCameraPreview();
                }
            }

            if (LiveFeedPanel.Visibility == Visibility.Visible)
            {
                LiveFeedText.Text = "Start Preview.";
            }
        }

        async void UploadAndDetectFaces(string imageFile)
        {
            try
            {
                var storageFile = await Windows.Storage.KnownFolders.PicturesLibrary.GetFileAsync(imageFile);
                var randomAccessStream = await storageFile.OpenReadAsync();

                using (Stream stream = randomAccessStream.AsStreamForRead())
                {
                    //this is the fragment where face is recognized:
                    var faces = await faceServiceClient.DetectAsync(stream, true);
                    var faceRects = faces.Select(face => face.FaceRectangle);
                    
                    _faceRectangles = faceRects.ToArray();
                    if (_faceRectangles.Length == 0)
                    {
                        PhotoLiveFeedText.Text = "Not Detected.";
                        PhotoLiveFeedPanel.Visibility = Visibility.Collapsed;
                        DisabledPhotoGrid.Visibility = Visibility.Visible;
                        // If a face is not detected, the file will be deleted.
                        await storageFile.DeleteAsync();
                    }
                    else
                    {

                        // 얼굴과 기존 DB와 분석해서 등록된 유저인지 판단해보자.
                        foreach (var faceObj in faces)
                        {
                            _faceId = faceObj.FaceId;
                            Debug.WriteLine("Find the face {0}", _faceId);
                            checkFacefromUserLists(_faceId);                            
                        }
                        //CustomCanvas.Invalidate();
                    }

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            AnalysingVisitorGrid.Visibility = Visibility.Collapsed;
            isSensor = false;
        }

        private async void checkFacefromUserLists(Guid id)
        {
            var userCnt = 0;
            double percent = 0;
            try
            {
                Debug.WriteLine("newFaceID :" + id);
                Debug.WriteLine("userIDs : " + _facesId[0]);
                var result = await faceServiceClient.FindSimilarAsync(id, _facesId, FindSimilarMatchMode.matchPerson);

                listView.Items.Clear();
                foreach (var fr in result)
                {
                    percent = fr.Confidence * 100;
                    Debug.WriteLine(fr.Confidence * 100 + "% matched");
                    listView.Items.Add(fr.Confidence * 100 + "% matched");
                    if (fr.Confidence >= OpenAPIConstants.precision)
                    {
                        userCnt++;
                    }
                }

                if(userCnt == 0)
                {
                    strangerDetected = true;
                }
            }
            catch(FaceAPIException ex)
            {
                Debug.WriteLine("Error Respons : {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
            }

            if (strangerDetected == true)
            {
                PhotoLiveFeedText.Text = "CAUTION !!!";
            }
            else
            {
                PhotoLiveFeedText.Text = userCnt + " User Detected";
            }

            strangerDetected = false;

        }

        private async void takePhoto_click(object sender, RoutedEventArgs e)
        {
            AnalysingVisitorGrid.Visibility = Visibility.Visible;
            actionLed();
            string time = DateTime.Now.ToString("yyyyMMddHHMMss");
            fileName = string.Format("capture_{0}.jpeg", time);

            photoFile = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFileAsync(
                            fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);


            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();

            await webcam.mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photoFile);

            IRandomAccessStream photoStream = await photoFile.OpenReadAsync();
            BitmapImage bitmap = new BitmapImage();
            bitmap.SetSource(photoStream);
            PhotoLiveFeedPanel.Visibility = Visibility.Visible;
            DisabledPhotoGrid.Visibility = Visibility.Collapsed;

            captureimage.Source = bitmap;

            Debug.WriteLine("fileName : " + fileName);
            Debug.WriteLine("PATH : " + photoFile.Path);

            UploadAndDetectFaces(fileName);
            actionLed();
        }

        private void RemovePhoto_click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Remove File !!!");
        }

    
        private async void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            Debug.WriteLine("Pin_ValueChanged");
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                Debug.WriteLine("Signal Detect(Rising)");
                //lcd.WriteLine(" ");
                //lcd.WriteLine("Detected");
                if (isSensor == false)
                {
                    isSensor = true;
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await TakePhoto();
                    });

                }
            }

            if (args.Edge == GpioPinEdge.FallingEdge)
            {
                Debug.WriteLine("Signal Detect(Falling)");
              //  lcd.WriteLine(" ");
               // lcd.WriteLine("Clear");
            }
        }

        private async Task TakePhoto()
        {
            Debug.WriteLine("Call TaskPhoto due to sensor signal");
            AnalysingVisitorGrid.Visibility = Visibility.Visible;

            actionLed();
            string time = DateTime.Now.ToString("yyyyMMddHHMMss");
            fileName = string.Format("capture_{0}.jpeg", time);

            photoFile = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFileAsync(
                            fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);


            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();

            await webcam.mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photoFile);

            IRandomAccessStream photoStream = await photoFile.OpenReadAsync();
            BitmapImage bitmap = new BitmapImage();
            bitmap.SetSource(photoStream);
            PhotoLiveFeedPanel.Visibility = Visibility.Visible;
            DisabledPhotoGrid.Visibility = Visibility.Collapsed;

            captureimage.Source = bitmap;

            Debug.WriteLine("fileName : " + fileName);
            Debug.WriteLine("PATH : " + photoFile.Path);

            UploadAndDetectFaces(fileName);
            actionLed();
        }
        /// <summary>
        /// Triggered when the whitelisted users grid is loaded. Sets the size of each photo within the grid.
        /// </summary>
        private void WhitelistedUsersGrid_Loaded(object sender, RoutedEventArgs e)
        {
            visitorIDPhotoGridMaxWidth = (WhitelistedUsersGrid.ActualWidth / 3) - 10;
        }
        /// <summary>
        /// Called when user hits vitual add user button. Navigates to NewUserPage page.
        /// </summary>
        private async void NewUserButton_Click(object sender, RoutedEventArgs e)
        {
            // Stops camera preview on this page, so that it can be started on NewUserPage
            await webcam.StopCameraPreview();

            //Navigates to NewUserPage, passing through initialized WebcamHelper object
            Frame.Navigate(typeof(NewUserPage), webcam);
        }

        private async void UpdateWhitelistedVisitors()
        {
            // If the whitelist isn't already being updated, update the whitelist
            if (!currentlyUpdatingWhitelist)
            {
                currentlyUpdatingWhitelist = true;
                await UpdateWhitelistedVisitorsList();
                UpdateWhitelistedVisitorsGrid();
                currentlyUpdatingWhitelist = false;
            }
        }
        
        /// <summary>
        /// Updates the list of Visitor objects with all whitelisted visitors stored on disk
        /// </summary>
        private async Task UpdateWhitelistedVisitorsList()
        {
            // Clears whitelist
            whitelistedVisitors.Clear();

            // If the whitelistFolder has not been opened, open it
            if (whitelistFolder == null)
            {
                whitelistFolder = await Windows.Storage.KnownFolders.PicturesLibrary.CreateFolderAsync(OpenAPIConstants.WhiteListFolderName, CreationCollisionOption.OpenIfExists);
            }

            // Populates subFolders list with all sub folders within the whitelist folders.
            // Each of these sub folders represents the Id photos for a single visitor.
            var subFolders = await whitelistFolder.GetFoldersAsync();

            // Iterate all subfolders in whitelist
            foreach (StorageFolder folder in subFolders)
            {
                string visitorName = folder.Name;
                var filesInFolder = await folder.GetFilesAsync();

                var photoStream = await filesInFolder[0].OpenAsync(FileAccessMode.Read);
                BitmapImage visitorImage = new BitmapImage();
                await visitorImage.SetSourceAsync(photoStream);

                Visitor whitelistedVisitor = new Visitor(visitorName, folder, visitorImage, visitorIDPhotoGridMaxWidth);

                whitelistedVisitors.Add(whitelistedVisitor);
            }
        }

        /// <summary>
        /// Updates UserInterface list of whitelisted users from the list of Visitor objects (WhitelistedVisitors)
        /// </summary>
        private void UpdateWhitelistedVisitorsGrid()
        {
            // Reset source to empty list
            WhitelistedUsersGrid.ItemsSource = new List<Visitor>();
            // Set source of WhitelistedUsersGrid to the whitelistedVisitors list
            WhitelistedUsersGrid.ItemsSource = whitelistedVisitors;

            // Hide Oxford loading ring
            OxfordLoadingRing.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Triggered when the user selects a visitor in the WhitelistedUsersGrid 
        /// </summary>
        private void WhitelistedUsersGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to UserProfilePage, passing through the selected Visitor object and the initialized WebcamHelper as a parameter
            Frame.Navigate(typeof(UserProfilePage), new UserProfileObject(e.ClickedItem as Visitor, webcam));
        }

        private void WhitelistedUsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }

    


}
