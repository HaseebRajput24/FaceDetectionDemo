using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VideoFrameAnalyzer;
using Window = System.Windows.Window;
using FaceAPI = Microsoft.Azure.CognitiveServices.Vision.Face;
using VisionAPI = Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Newtonsoft.Json;
using System.Drawing;
using System.Reflection;
using System.IO;

namespace FaceDetectionApp
{
   
    public partial class MainWindow : Window
    {
        private readonly FrameGrabber<LiveCameraResult> _grabber;
        private readonly CascadeClassifier _localFaceDetector = new CascadeClassifier();
        
        private FaceAPI.FaceClient _faceClient = null;
        private static readonly ImageEncodingParam[] s_jpegParams = {
            new ImageEncodingParam(ImwriteFlags.JpegQuality, 60)
        };
        private readonly string FaceAPIKey= "";
        private readonly string FaceAPIURL = "";
       
        public static int count = 0;
        public static int Callcount = 0;
        public MainWindow()
        {
            InitializeComponent();
           
            
            DetectedPersonCount.Text = "Number of Person Detected: " + count;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // Create grabber. 
            _grabber = new FrameGrabber<LiveCameraResult>();

            // Set up a listener for when the client receives a new frame.
            _grabber.NewFrameProvided += (s, e) =>
            {
                
                var rects = _localFaceDetector.DetectMultiScale(e.Frame.Image);
                e.Frame.UserData = rects;
                              
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                                     
                    Image.Source = e.Frame.Image.ToBitmapSource();                 
                }));
                
               

            };

            
            DirectoryInfo dir = Directory.GetParent(Environment.CurrentDirectory).Parent;
            string dirPath = dir.FullName;
            _localFaceDetector.Load(dirPath+"\\Data\\haarcascade_frontalface_alt2.xml");          
            _grabber.AnalysisFunction = EmotionAnalysisFunction;


        }

        private async Task<LiveCameraResult> EmotionAnalysisFunction(VideoFrame frame)
        {
            // Encode image. 
            var jpg = frame.Image.ToMemoryStream(".jpg", s_jpegParams);

      
            FaceAPI.Models.DetectedFace[] faces = null;

            // See if we have local face detections for this image.
            var localFaces = (OpenCvSharp.Rect[])frame.UserData;
           
            if (localFaces == null || localFaces.Count() > 0)
            {
                await this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    DetectedPersonCount.Text = "Number of Person Detected: " + (++count);
                }));
                faces = (await _faceClient.Face.DetectWithStreamAsync(jpg, recognitionModel: "recognition_04")).ToArray();
                List<FaceInfo> list = new List<FaceInfo>();
                foreach(FaceAPI.Models.DetectedFace face in faces)
                {
                    list.Add(new FaceInfo() { FaceId = face.FaceId.ToString() });
                }
                
            }
            else
            {
                // Local face detection found no faces; don't call Cognitive Services.
                faces = new FaceAPI.Models.DetectedFace[0];
            }

            // Output. 
            return new LiveCameraResult
            {
                Faces = faces
            };
        }
        public System.Drawing.Image ResizeWithSameRatio(System.Drawing.Image image, float width, float height)
        {
            // the colour for letter boxing, can be a parameter
            var brush = new SolidBrush(System.Drawing.Color.Black);

            // target scaling factor
            float scale = Math.Min(width / image.Width, height / image.Height);

            // target image
            var bmp = new Bitmap((int)width, (int)height);
            var graph = Graphics.FromImage(bmp);

            var scaleWidth = (int)(image.Width * scale);
            var scaleHeight = (int)(image.Height * scale);

            // fill the background and then draw the image in the 'centre'
            graph.FillRectangle(brush, new RectangleF(0, 0, width, height));
            graph.DrawImage(image, new System.Drawing.Rectangle(((int)width - scaleWidth) / 2, ((int)height - scaleHeight) / 2, scaleWidth, scaleHeight));

            return bmp;
        }
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CameraList.HasItems)
            {
               MessageArea.Text = "No cameras found; cannot start processing";
               return;
            }

           
            _faceClient = new FaceAPI.FaceClient(new FaceAPI.ApiKeyServiceClientCredentials(FaceAPIKey))
            {
                Endpoint = FaceAPIURL
            };
          

            // How often to analyze. 
            _grabber.TriggerAnalysisOnInterval(TimeSpan.FromSeconds(3));


            await _grabber.StartProcessingCameraAsync(CameraList.SelectedIndex);
        }
        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            await _grabber.StopProcessingAsync();
        }
       

        private void CameraList_Loaded(object sender, RoutedEventArgs e)
        {
            int numCameras = _grabber.GetNumCameras();

            if (numCameras == 0)
            {
               // MessageArea.Text = "No cameras found!";
            }

            var comboBox = sender as ComboBox;
            comboBox.ItemsSource = Enumerable.Range(0, numCameras).Select(i => string.Format("Camera {0}", i + 1));
            comboBox.SelectedIndex = 0;
        }




    }

    public class FaceInfo
    {
        public string FaceId { get; set; }
    }

   
}
