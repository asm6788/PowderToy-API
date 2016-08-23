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
using System.Timers;

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

    public class PowderToyComment
    {
        public String Username = "";
        public string Comment = "";
        public Savestauts stauts = Savestauts.None;
        public TimeSpan Time = new TimeSpan();
        public int ID = 0;
        public int Score = 0;
        public int VoteUp = 0;
        public int VoteDown = 0;
        public int Hits = 0;
        public string Title = "";
        public string Explanation = "";
        public bool IsPublic = true;
        public string Maker = "";
        public int CommentCount = 0;
        int totalpage = 0;
        Timer timer = new Timer();


        public PowderToyComment()
        {

        }
        public PowderToyComment(int ID, string Username, string Comment, Savestauts stauts, TimeSpan Time, int Score, int Voteup, int VoteDown, int Hits, string Title, string Explanation, bool IsPublic, string Maker, int TotalPage)
        {
            this.Username = Username;
            this.Comment = Comment;
            this.stauts = stauts;
            this.Time = Time;
            this.ID = ID;
            this.Score = Score;
            this.VoteUp = Voteup;
            this.VoteDown = VoteDown;
            this.Hits = Hits;
            this.Title = Title;
            this.Explanation = Explanation;
            this.IsPublic = IsPublic;
            this.Maker = Maker;
            this.totalpage = TotalPage;

        }
        public PowderToyComment(int ID)
        {
            this.ID = ID;
            HttpWebRequest wReq;
            HttpWebResponse wRes;

            WebRequest requestPic = WebRequest.Create("http://static.powdertoy.co.uk/" + ID + ".png");

            WebResponse responsePic = requestPic.GetResponse();

            Uri uri = new Uri("http://powdertoy.co.uk/Browse/View.json?ID=" + ID); // string 을 Uri 로 형변환
            wReq = (HttpWebRequest)WebRequest.Create(uri); // WebRequest 객체 형성 및 HttpWebRequest 로 형변환
            wReq.Method = "GET"; // 전송 방법 "GET" or "POST"
            wReq.ServicePoint.Expect100Continue = false;
            wReq.CookieContainer = new CookieContainer();
            string res = null;

            using (wRes = (HttpWebResponse)wReq.GetResponse())
            {
                Stream respPostStream = wRes.GetResponseStream();
                StreamReader readerPost = new StreamReader(respPostStream, Encoding.GetEncoding("EUC-KR"), true);

                res = readerPost.ReadToEnd();
            }

            JsonTextParser parser = new JsonTextParser();
            JsonObject obj = parser.Parse(res);

            JsonUtility.GenerateIndentedJsonText = false;


            int i = 0;
            String[] View = null;
            View = new string[17];
            foreach (JsonObject field in obj as JsonObjectCollection)
            {
                i++;
                string name = field.Name;
                string value = string.Empty;
                string type = field.GetValue().GetType().Name;

                // try to get value.
                switch (type)
                {
                    case "String":
                        value = (string)field.GetValue();
                        break;

                    case "Double":
                        value = field.GetValue().ToString();
                        break;

                    case "Boolean":
                        value = field.GetValue().ToString();
                        break;

                }
                View[i] = value;

            }
            Score = Convert.ToInt32(View[3]);
            VoteUp = Convert.ToInt32(View[4]);
            VoteDown = Convert.ToInt32(View[5]);
            Hits = Convert.ToInt32(View[6]);
            Title = View[8];
            Explanation = View[9];
            TimeSpan t = TimeSpan.FromSeconds(Convert.ToInt32(View[11]));
            int hour = t.Hours + 9;
            if (hour > 24)
            {
                hour = hour - 24;
                if (hour >= 12)
                    hour = hour + 12;
            }

            this.Time = new TimeSpan(hour, t.Minutes, t.Seconds);
            Maker = View[12];
            CommentCount = Convert.ToInt32(View[13]);
            if (Convert.ToInt32(View[13]) / 20 == 0)
            {
                totalpage = Convert.ToInt32(View[13]);
            }
            else if (Convert.ToInt32(View[13]) / 20 == Convert.ToInt32(Convert.ToInt32(View[13]) / 20))
            {
                int one = Convert.ToInt32(View[13]) / 20 + 1;
                totalpage = one;
            }
            else
            {
                totalpage = Convert.ToInt32(View[13]) / 20;
            }
            IsPublic = Convert.ToBoolean(View[14]);
        }
        public List<PowderToyComment> GetAll()
        {
            List<PowderToyComment> powder = new List<PowderToyComment>();
            int CommentCount = 0;
            int i = 0;
            double temp = 0;
            List<string> Username = new List<string>();
            List<string> CommentText = new List<string>();
            List<string> Date = new List<string>();
            if (totalpage == 0)
                throw new Exception("No Initialization");
            else
                while (true)
                {
                    CommentCount += 20;
                    HttpWebRequest wReq;
                    HttpWebResponse wRes;
                    int startcount = CommentCount - 20;
                    Uri uri = new Uri("http://powdertoy.co.uk/Browse/Comments.json?ID=" + ID + "&Start=" + startcount + "&Count=20"); // string 을 Uri 로 형변환
                    wReq = (HttpWebRequest)WebRequest.Create(uri); // WebRequest 객체 형성 및 HttpWebRequest 로 형변환
                    wReq.Method = "GET"; // 전송 방법 "GET" or "POST"
                    wReq.ServicePoint.Expect100Continue = false;
                    wReq.CookieContainer = new CookieContainer();
                    string res = null;

                    if ((i / ((double)totalpage * 20)) + 0.01 == temp)
                    {
                        goto Out;
                    }
                    else
                    {
                        temp = (i / ((double)totalpage * 20)) + 0.01;
                    }

                    using (wRes = (HttpWebResponse)wReq.GetResponse())
                    {
                        Stream respPostStream = wRes.GetResponseStream();
                        StreamReader readerPost = new StreamReader(respPostStream, Encoding.GetEncoding("EUC-KR"), true);

                        res = readerPost.ReadToEnd();
                    }
                    JsonTextParser parser = new JsonTextParser();
                    JsonObject obj = parser.Parse(res);
                    JsonArrayCollection col = (JsonArrayCollection)obj;



                    foreach (JsonObjectCollection joc in col)
                    {
                        i++;
                        Username.Add((string)joc["Username"].GetValue());
                        CommentText.Add((string)joc["Text"].GetValue());
                        Date.Add((string)joc["Timestamp"].GetValue());
                        TimeSpan t = TimeSpan.FromSeconds(Convert.ToInt32(Date[Date.Count - 1]));
                        int hour = t.Hours + 9;
                        if (hour > 24)
                        {
                            hour = hour - 24;
                            if (hour >= 12)
                                hour = hour + 12;
                        }
                        powder.Add(new PowderToyComment(ID, Username[Username.Count - 1], CommentText[CommentText.Count - 1], Savestauts.OK, t, Score, VoteUp, VoteDown, Hits, Title, Explanation, IsPublic, Maker, totalpage));

                    }
                }
            Out:
            return powder;

        }
        public void Alarm(int Elapsed)
        {
            timer.Interval = Elapsed;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //HttpWebRequest wReq;
            //HttpWebResponse wRes;
            //int count = CommentCount + 20;
            //Uri uri = new Uri("http://powdertoy.co.uk/Browse/Comments.json?ID=" + ID + "& Start =" + CommentCount + "&Count=" + count); // string 을 Uri 로 형변환
            //wReq = (HttpWebRequest)WebRequest.Create(uri); // WebRequest 객체 형성 및 HttpWebRequest 로 형변환
            //wReq.Method = "GET"; // 전송 방법 "GET" or "POST"
            //wReq.ServicePoint.Expect100Continue = false;
            //wReq.CookieContainer = new CookieContainer();
            //string res = null;

            //using (wRes = (HttpWebResponse)wReq.GetResponse())
            //{
            //    Stream respPostStream = wRes.GetResponseStream();
            //    StreamReader readerPost = new StreamReader(respPostStream, Encoding.GetEncoding("EUC-KR"), true);

            //    res = readerPost.ReadToEnd();
            //}
            //JsonTextParser parser = new JsonTextParser();
            //JsonObject obj = parser.Parse(res);
            //JsonArrayCollection col = (JsonArrayCollection)obj;

            //string[] Username = new string[22];
            //string[] CommentText = new string[22];
            //string[] Date = new string[22];
            //int i = 0;
            //foreach (JsonObjectCollection joc in col)
            //{
            //    i++;
            //    Username[i] = (string)joc["Username"].GetValue();
            //    CommentText[i] = (string)joc["Text"].GetValue();
            //    Date[i] = (string)joc["Timestamp"].GetValue();
            //    Console.WriteLine(Username[i] + CommentText[i] + Date[i]);
            //    TimeSpan t = TimeSpan.FromSeconds(Convert.ToInt32(Date[i]));
            //    int hour = t.Hours + 9;
            //    if (hour > 24)
            //    {
            //        hour = hour - 24;
            //        if (hour >= 12)
            //            hour = hour + 12;
            //    }
            //    textBox3.AppendText("닉네임: " + Username[i] + "\r\n" + "날짜: " + hour + "시" + t.Minutes + "분" + t.Seconds + "초" + " 댓글: " + CommentText[i] + "\r\n\r\n");
            //}

            //if (전에[1] != CommentText[1])
            //{
            //    notifyIcon1.Visible = true; // 트레이의 아이콘을 보이게 한다.
            //    notifyIcon1.BalloonTipText = CommentText[1];
            //    notifyIcon1.ShowBalloonTip(500);
            //    전에 = CommentText;
            //    timer1.Start();
            //}
        }

        public enum Savestauts
        {
            None,
            DELTED,
            OK
        }
    }
}
