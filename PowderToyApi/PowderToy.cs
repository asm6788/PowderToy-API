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

            Console.WriteLine(string.Format("Uploading {0} to {1}", file, "http://powdertoy.co.uk/Save.api"));
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("http://powdertoy.co.uk/Save.api");
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Headers.Add("X-Auth-User-Id", USERID);
            wr.Headers.Add("X-Auth-Session-Key", SESSIONID);
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

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
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                string resopne = reader2.ReadToEnd();
                Console.WriteLine(string.Format("File uploaded, server response is: {0}", resopne));
                return Convert.ToInt32(resopne.Remove(0, 3));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uploading file", ex);
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }

            return 0;
        }
        public static void Auth(string ID, string PW)
        {
            String callUrl = "http://powdertoy.co.uk/Login.json";
            String[] data = new String[2];
            data[0] = "asm6788";         // id
            data[1] = MD5Hash(ID + "-" + MD5Hash(PW));          // pw

            String postData = String.Format("Username={0}&Hash={1}", data[0], data[1]);


            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(callUrl);
            // 인코딩 UTF-8
            byte[] sendData = UTF8Encoding.UTF8.GetBytes(postData);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = sendData.Length;
            Stream requestStream = httpWebRequest.GetRequestStream();
            requestStream.Write(sendData, 0, sendData.Length);
            requestStream.Close();
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
            String res = streamReader.ReadToEnd();
            Console.WriteLine(res);
            JsonTextParser parser = new JsonTextParser();
            JsonObject obj = parser.Parse(res);

            foreach (JsonObject field in obj as JsonObjectCollection)
            {
                string name = field.Name;
                string value = string.Empty;
                string type = field.GetValue().GetType().Name;

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
            streamReader.Close();
            httpWebResponse.Close();
        }
        public static void DeleteSAVE(int SaveID)
        {
            String callUrl = "http://powdertoy.co.uk/Browse/Delete.json?ID=" + SaveID + "&Mode=Delete" + "&Key=" + SESSIONKEY;

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(callUrl);
            // 인코딩 UTF-8
            httpWebRequest.Headers.Add("X-Auth-User-Id", USERID);
            httpWebRequest.Headers.Add("X-Auth-Session-Key", SESSIONID);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest.Method = "POST";
            Stream requestStream = httpWebRequest.GetRequestStream();
            requestStream.Close();
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
            String res = streamReader.ReadToEnd();
            Console.WriteLine(res);
            JsonTextParser parser = new JsonTextParser();
            JsonObject obj = parser.Parse(res);

            foreach (JsonObject field in obj as JsonObjectCollection)
            {
                string name = field.Name;
                string value = string.Empty;
                string type = field.GetValue().GetType().Name;

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
            streamReader.Close();
            httpWebResponse.Close();
        }
        public static void AddComment(int SaveID, string Comment)
        {
            String callUrl = "http://powdertoy.co.uk/Browse/Comments.json?ID=" + SaveID;
            String[] data = new String[1];
            data[0] = Comment;         // id
            String postData = String.Format("Comment={0}", data[0]);


            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(callUrl);
            // 인코딩 UTF-8
            byte[] sendData = UTF8Encoding.UTF8.GetBytes(postData);
            httpWebRequest.Headers.Add("X-Auth-User-Id", USERID);
            httpWebRequest.Headers.Add("X-Auth-Session-Key", SESSIONID);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = sendData.Length;
            Stream requestStream = httpWebRequest.GetRequestStream();
            requestStream.Write(sendData, 0, sendData.Length);
            requestStream.Close();
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
            String res = streamReader.ReadToEnd();
            Console.WriteLine(res);
            JsonTextParser parser = new JsonTextParser();
            JsonObject obj = parser.Parse(res);

            foreach (JsonObject field in obj as JsonObjectCollection)
            {
                string name = field.Name;
                string value = string.Empty;
                string type = field.GetValue().GetType().Name;

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
            streamReader.Close();
            httpWebResponse.Close();
        }
        public static void VoteUp(int SaveID)
        {
            String callUrl = "http://powdertoy.co.uk/Vote.api";
            String[] data = new String[2];
            data[0] = SaveID.ToString();         // id
            data[1] = "Up";
            String postData = String.Format("ID={0}&Action={1}", data[0], data[1]);


            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(callUrl);
            // 인코딩 UTF-8
            byte[] sendData = UTF8Encoding.UTF8.GetBytes(postData);
            httpWebRequest.Headers.Add("X-Auth-User-Id", USERID);
            httpWebRequest.Headers.Add("X-Auth-Session-Key", SESSIONID);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = sendData.Length;
            Stream requestStream = httpWebRequest.GetRequestStream();
            requestStream.Write(sendData, 0, sendData.Length);
            requestStream.Close();
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
            String res = streamReader.ReadToEnd();
            Console.WriteLine(res);
            streamReader.Close();
            httpWebResponse.Close();
        }
        public static void VoteDown(int SaveID)
        {
            String callUrl = "http://powdertoy.co.uk/Vote.api";
            String[] data = new String[2];
            data[0] = SaveID.ToString();         // id
            data[1] = "Down";
            String postData = String.Format("ID={0}&Action={1}", data[0], data[1]);


            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(callUrl);
            // 인코딩 UTF-8
            byte[] sendData = UTF8Encoding.UTF8.GetBytes(postData);
            httpWebRequest.Headers.Add("X-Auth-User-Id", USERID);
            httpWebRequest.Headers.Add("X-Auth-Session-Key", SESSIONID);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = sendData.Length;
            Stream requestStream = httpWebRequest.GetRequestStream();
            requestStream.Write(sendData, 0, sendData.Length);
            requestStream.Close();
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
            String res = streamReader.ReadToEnd();
            Console.WriteLine(res);
            streamReader.Close();
            httpWebResponse.Close();
        }
        public static string MD5Hash(string Data)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(Data));

            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hash)
            {
                stringBuilder.AppendFormat("{0:x2}", b);
            }
            return stringBuilder.ToString();
        }

    }
