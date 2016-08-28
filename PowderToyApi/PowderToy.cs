using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PowderToyApi
{
    static public class PowderToy
    {
        public static string USERID = "";
        private static string SESSIONID = "";
        private static string SESSIONKEY = "";

        public static int UploadSAVE(string Name, bool IsPublish, string Description, string file)
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("Name", Name);
            nvc.Add("Description", Description);
            if (IsPublish)
            {
                nvc.Add("Publish", "Public");
            }
            else
            {
                nvc.Add("Publish", "Private");
            }
            if (Name.Trim() == "")
            {
                throw new Exception("Name is blank");
            }
            else if (Description.Trim() == "")
            {
                throw new Exception("Description is blank");
            }
            try
            {
                if (File.ReadAllLines(file)[0].Remove(4) != "OPS1")
                {
                    throw new Exception("Invalid SaveFile");
                }
            }
            catch
            {
                throw new Exception("Invalid SaveFile");
            }
            Console.WriteLine(File.ReadAllLines(file)[0].Remove(4));

            Console.WriteLine(string.Format("Uploading {0} to {1}", file, "http://powdertoy.co.uk/Save.api"));
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = WebRequest.Create("http://powdertoy.co.uk/Save.api") as HttpWebRequest;
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Headers.Add("X-Auth-User-Id", USERID);
            wr.Headers.Add("X-Auth-Session-Key", SESSIONID);
            wr.Credentials = CredentialCache.DefaultCredentials;

            using (Stream rs = wr.GetRequestStream())
            {

                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                foreach (string key in nvc.Keys)
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                    string formitem = string.Format(formdataTemplate, key, nvc[key]);
                    byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                    rs.Write(formitembytes, 0, formitembytes.Length);
                }
                rs.Write(boundarybytes, 0, boundarybytes.Length);

                string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                string header = string.Format(headerTemplate, "Data", file, "binary");
                byte[] headerbytes = Encoding.UTF8.GetBytes(header);
                rs.Write(headerbytes, 0, headerbytes.Length);

                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        rs.Write(buffer, 0, bytesRead);
                    }
                }

                byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                rs.Write(trailer, 0, trailer.Length);
            }

            WebResponse wresp = null;
            wresp = wr.GetResponse();
            Stream stream2 = wresp.GetResponseStream();
            StreamReader reader2 = new StreamReader(stream2);
            string resopne = reader2.ReadToEnd();
            Console.WriteLine(string.Format("File uploaded, server response is: {0}", resopne));
            return Convert.ToInt32(resopne.Remove(0, 3));
        }
        public static void Auth(string ID, string PW)
        {
            string callUrl = "http://powdertoy.co.uk/Login.json";

            string postData = string.Format("Username={0}&Hash={1}", ID, MD5Hash(ID + "-" + MD5Hash(PW)));


            HttpWebRequest httpWebRequest = WebRequest.Create(callUrl) as HttpWebRequest;
            // 인코딩 UTF-8
            byte[] sendData = Encoding.UTF8.GetBytes(postData);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = sendData.Length;
            using (Stream requestStream = httpWebRequest.GetRequestStream())
                requestStream.Write(sendData, 0, sendData.Length);

            HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8")))
            {
                string res = streamReader.ReadToEnd();
                Console.WriteLine(res);
                JsonTextParser parser = new JsonTextParser();
                JsonObject obj = parser.Parse(res);

                foreach (JsonObject field in obj as JsonObjectCollection)
                {
                    if (field.Name == "Error")
                    {
                        throw new Exception(field.GetValue().ToString());
                    }
                    else if (field.Name == "UserID")
                    {
                        USERID = field.GetValue().ToString();
                    }
                    else if (field.Name == "SessionID")
                    {
                        SESSIONID = field.GetValue().ToString();
                    }
                    else if (field.Name == "SessionKey")
                    {
                        SESSIONKEY = field.GetValue().ToString();
                    }
                }
                httpWebResponse.Close();
            }
        }
        public static void DeleteSAVE(int SaveID)
        {
            string callUrl = "http://powdertoy.co.uk/Browse/Delete.json?ID=" + SaveID + "&Mode=Delete" + "&Key=" + SESSIONKEY;

            HttpWebRequest httpWebRequest = WebRequest.Create(callUrl) as HttpWebRequest;
            // 인코딩 UTF-8
            httpWebRequest.Headers.Add("X-Auth-User-Id", USERID);
            httpWebRequest.Headers.Add("X-Auth-Session-Key", SESSIONID);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest.Method = "POST";
            Stream requestStream = httpWebRequest.GetRequestStream();
            requestStream.Close();
            HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8")))
            {
                string res = streamReader.ReadToEnd();
                Console.WriteLine(res);
                JsonTextParser parser = new JsonTextParser();
                JsonObject obj = parser.Parse(res);

                foreach (JsonObject field in obj as JsonObjectCollection)
                {
                    if (field.Name == "Error")
                    {
                        throw new Exception(field.GetValue().ToString());
                    }
                    else if (field.Name == "UserID")
                    {
                        USERID = field.GetValue().ToString();
                    }
                    else if (field.Name == "SessionID")
                    {
                        SESSIONID = field.GetValue().ToString();
                    }
                    else if (field.Name == "SessionKey")
                    {
                        SESSIONKEY = field.GetValue().ToString();
                    }
                }
            }
            httpWebResponse.Close();
        }
        public static void AddComment(int SaveID, string Comment)
        {
            string callUrl = "http://powdertoy.co.uk/Browse/Comments.json?ID=" + SaveID;
            string data = Comment;         // id
            string postData = string.Format("Comment={0}", data);


            HttpWebRequest httpWebRequest = WebRequest.Create(callUrl) as HttpWebRequest;
            // 인코딩 UTF-8
            byte[] sendData = Encoding.UTF8.GetBytes(postData);
            httpWebRequest.Headers.Add("X-Auth-User-Id", USERID);
            httpWebRequest.Headers.Add("X-Auth-Session-Key", SESSIONID);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = sendData.Length;
            using (Stream requestStream = httpWebRequest.GetRequestStream())
                requestStream.Write(sendData, 0, sendData.Length);

            HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8")))
            {
                string res = streamReader.ReadToEnd();
                Console.WriteLine(res);

                if (res == "Error: 404")
                {
                    throw new Exception("Invalid ID");
                }

                JsonTextParser parser = new JsonTextParser();
                JsonObject obj = parser.Parse(res);

                foreach (JsonObject field in obj as JsonObjectCollection)
                    if (field.Name == "UserID")
                    {
                        USERID = field.GetValue().ToString();
                    }
                    else if (field.Name == "SessionID")
                    {
                        SESSIONID = field.GetValue().ToString();
                    }
                    else if (field.Name == "SessionKey")
                    {
                        SESSIONKEY = field.GetValue().ToString();
                    }
            }
            httpWebResponse.Close();
        }
        public static void VoteUp(int SaveID)
        {
            string callUrl = "http://powdertoy.co.uk/Vote.api";
            string[] data = new string[2];
            data[0] = SaveID.ToString();         // id
            data[1] = "Up";
            string postData = string.Format("ID={0}&Action={1}", data[0], data[1]);


            HttpWebRequest httpWebRequest = WebRequest.Create(callUrl) as HttpWebRequest;
            // 인코딩 UTF-8
            byte[] sendData = Encoding.UTF8.GetBytes(postData);
            httpWebRequest.Headers.Add("X-Auth-User-Id", USERID);
            httpWebRequest.Headers.Add("X-Auth-Session-Key", SESSIONID);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = sendData.Length;
            using (Stream requestStream = httpWebRequest.GetRequestStream())
                requestStream.Write(sendData, 0, sendData.Length);

            HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8")))
            {
                string res = streamReader.ReadToEnd();
                if (res == "No such save exists")
                {
                    throw new Exception("No such save exists");
                }
                Console.WriteLine(res);
            }
            httpWebResponse.Close();
        }
        public static void VoteDown(int SaveID)
        {
            string callUrl = "http://powdertoy.co.uk/Vote.api";
            string[] data = new string[2];
            data[0] = SaveID.ToString();         // id
            data[1] = "Down";
            string postData = string.Format("ID={0}&Action={1}", data[0], data[1]);


            HttpWebRequest httpWebRequest = WebRequest.Create(callUrl) as HttpWebRequest;
            // 인코딩 UTF-8
            byte[] sendData = Encoding.UTF8.GetBytes(postData);
            httpWebRequest.Headers.Add("X-Auth-User-Id", USERID);
            httpWebRequest.Headers.Add("X-Auth-Session-Key", SESSIONID);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = sendData.Length;
            using (Stream requestStream = httpWebRequest.GetRequestStream())
                requestStream.Write(sendData, 0, sendData.Length);

            HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8")))
            {
                string res = streamReader.ReadToEnd();
                if (res == "No such save exists")
                {
                    throw new Exception("No such save exists");
                }
                Console.WriteLine(res);
            }
            httpWebResponse.Close();
        }
        public static string MD5Hash(string Data)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(Data));
            StringBuilder stringBuilder = new StringBuilder();
            return (from b in hash select stringBuilder.AppendFormat("{0:x2}", b)).Last().ToString();
        }

    }
}
