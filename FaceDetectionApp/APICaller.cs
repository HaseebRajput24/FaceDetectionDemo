using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FaceDetectionApp
{
    public class APICaller
    {
        private static APICaller instance;
        public static APICaller Instance
        {
            get
            {
                if(instance==null)
                {
                    instance = new APICaller();
                }
                return instance;
            }
        }
        public  string CallMosinoAPI(string URL,string RequestType, string data = "")
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(URL);              
                webRequest.Method = RequestType;
                webRequest.ContentType = "application/json";
                if (data != "" && RequestType != "GET")
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(data);
                    webRequest.ContentLength = byteArray.Length;
                    Stream postStreamdata = webRequest.GetRequestStream();
                    postStreamdata.Write(byteArray, 0, byteArray.Length);
                    postStreamdata.Close();
                }


                WebResponse response = webRequest.GetResponse();
                Stream postStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(postStream);
                string responseFromServer = reader.ReadToEnd();
                return responseFromServer;

            }
            catch (Exception ex)
            {
                return "-1";
            }
        }
    }
}
