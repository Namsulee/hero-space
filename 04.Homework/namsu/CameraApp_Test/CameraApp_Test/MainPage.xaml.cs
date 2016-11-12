using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;


// 빈 페이지 항목 템플릿은 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 에 문서화되어 있습니다.

namespace CameraApp_Test
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("6dd536fee25044a6a402f88a67cd93d1");
        FaceRectangle[] _faceRectangles;
        

        public MainPage()
        {
            this.InitializeComponent();
            UploadAndDetectFaces("ms-appx:///Assets/StoreLogo.png");
        }

        async void UploadAndDetectFaces(string imageFilePath)
        {
            try
            {
                StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
                var storageFile = await assets.GetFileAsync("20160509092907755236.jpg");
                var randomAccessStream = await storageFile.OpenReadAsync();

                using (Stream stream = randomAccessStream.AsStreamForRead())
                {
                    //this is the fragment where face is recognized:
                    var faces = await faceServiceClient.DetectAsync(stream);
                    var faceRects = faces.Select(face => face.FaceRectangle);
                    _faceRectangles = faceRects.ToArray();
                    CustomCanvas.Invalidate();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        void canvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (_faceRectangles != null)
                if (_faceRectangles.Length > 0)
                {
                    foreach (var faceRect in _faceRectangles)
                    {
                        args.DrawingSession.DrawRectangle(faceRect.Left, faceRect.Top, faceRect.Width, faceRect.Height, Colors.Red);
                    }
                }
        }
    }
}
