using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FaceAPI = Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using VisionAPI = Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace FaceDetectionApp
{
   
        public class LiveCameraResult
        {
            public FaceAPI.DetectedFace[] Faces { get; set; } = null;
            public string[] CelebrityNames { get; set; } = null;
            public VisionAPI.ImageTag[] Tags { get; set; } = null;
        }
    

}
