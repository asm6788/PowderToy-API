﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Json;
using System.Text;
using System.Timers;

namespace PowderToyApi
{
    public class NewCommentEventArgs : EventArgs
    {
        public string eventData;

        public NewCommentEventArgs(string eventData)
        {
            this.eventData = eventData;
        }
    }

    public class PowderToyComment
    {
        public string Username = "";
        public string Comment = "";
        public DateTime Time = new DateTime();
        public int ID = 0;
        int totalpage = 0;
        public int Elapsed = 0;
        string[] 전에 = null;
        Timer timer = new Timer();

        public event EventHandler<PowderToyComment> Newcomment; //닷넷에 미리 정의된 System.EventHandler 델리게이트 이용
        public void DoClick(PowderToyComment data)
        {
            Newcomment?.Invoke(null, data); //System.EventHandler은 두개의 매개변수를 요구함(일단 null 처리)
        }

        public PowderToyComment()
        {

        }
        public PowderToyComment(int ID, string Username, string Comment, DateTime Time)
        {
            this.Username = Username;
            this.Comment = Comment;
            this.Time = Time;
            this.ID = ID;
        }

        public PowderToyComment(int ID, string Username, string Comment, DateTime Time, bool IsAlarm, int Elapsed)
        {
            this.Username = Username;
            this.Comment = Comment;
            this.Time = Time;
            this.ID = ID;
            this.Elapsed = Elapsed;
            Alarm(Elapsed);
        }


        public PowderToyComment(int ID)
        {
            this.ID = ID;
            HttpWebRequest wReq;
            HttpWebResponse wRes;

            WebRequest requestPic = WebRequest.Create("http://static.powdertoy.co.uk/" + ID + ".png");

            WebResponse responsePic = requestPic.GetResponse();

            Uri uri = new Uri("http://powdertoy.co.uk/Browse/View.json?ID=" + ID); // string으로 URI 생성
            wReq = WebRequest.Create(uri) as HttpWebRequest; // WebRequest 객체 형성 및 HttpWebRequest 로 형변환
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

            if (res == "Error: 404")
            {
                throw new Exception("Invalid ID");
            }
            JsonTextParser parser = new JsonTextParser();
            JsonObject obj = parser.Parse(res);

            JsonUtility.GenerateIndentedJsonText = false;


            int i = 0;
            string[] View = new string[17];
            foreach (JsonObject field in obj as JsonObjectCollection)
            {
                i++;
                string value = string.Empty;
                string type = field.GetValue().GetType().Name;
                // try to get value.
                switch (type)
                {
                    case "String":
                        value = field.GetValue() as string;
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
            TimeSpan t = TimeSpan.FromSeconds(Convert.ToInt32(View[11]));

            int hour = t.Hours + 9;
            if (hour > 24)
            {
                hour = hour - 24;
                if (hour >= 12)
                    hour = hour + 12;
            }

            DateTime Date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + t;
            Time = new DateTime(Date.Year, Date.Month, Date.Day, hour, t.Minutes, t.Seconds, t.Milliseconds);


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

        }
        public List<PowderToyComment> GetAll()
        {
            List<PowderToyComment> powder = new List<PowderToyComment>();
            int CommentCount = 0;
            int i = 0;
            double temp = 0;


            if (totalpage == 0)
                throw new Exception("No Initialization");
            else
                while (true)
                {
                    CommentCount += 20;
                    HttpWebRequest wReq;
                    HttpWebResponse wRes;
                    int startcount = CommentCount - 20;
                    Uri uri = new Uri("http://powdertoy.co.uk/Browse/Comments.json?ID=" + ID + "&Start=" + startcount + "&Count=20"); // string으로 URI 생성
                    wReq = WebRequest.Create(uri) as HttpWebRequest; // WebRequest 객체 형성 및 HttpWebRequest 로 형변환
                    wReq.Method = "GET"; // 전송 방법 "GET" or "POST"
                    wReq.ServicePoint.Expect100Continue = false;
                    wReq.CookieContainer = new CookieContainer();
                    string res = null;

                    if ((i / ((double)totalpage * 20)) + 0.01 == temp)
                        break;
                    else
                        temp = (i / ((double)totalpage * 20)) + 0.01;

                    using (wRes = (HttpWebResponse)wReq.GetResponse())
                    {
                        Stream respPostStream = wRes.GetResponseStream();
                        StreamReader readerPost = new StreamReader(respPostStream, Encoding.GetEncoding("EUC-KR"), true);

                        res = readerPost.ReadToEnd();
                    }
                    JsonTextParser parser = new JsonTextParser();
                    JsonObject obj = parser.Parse(res);
                    JsonArrayCollection col = (JsonArrayCollection)obj;


                    List<string> Username = new List<string>();
                    List<string> CommentText = new List<string>();
                    List<string> Timestamp = new List<string>();
                    foreach (JsonObjectCollection joc in col)
                    {
                        i++;
                        Console.WriteLine(Convert.ToInt32((i / ((double)totalpage * 20)) * 100));
                        Username.Add(joc["Username"].GetValue() as string);
                        CommentText.Add(joc["Text"].GetValue() as string);
                        Timestamp.Add(joc["Timestamp"].GetValue() as string);
                        Console.WriteLine(Username[Username.Count - 1] + CommentText[CommentText.Count - 1] + Timestamp[Timestamp.Count - 1]);
                        TimeSpan t = TimeSpan.FromSeconds(Convert.ToInt32(Timestamp[Timestamp.Count - 1]));
                        int hour = t.Hours + 9;
                        if (hour > 24)
                        {
                            hour = hour - 24;
                            if (hour >= 12)
                                hour = hour + 12;
                        }
                        DateTime Date1 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + t;
                        List<DateTime> Date = new List<DateTime>();
                        Date.Add(new DateTime(Date1.Year, Date1.Month, Date1.Day, hour, t.Minutes, t.Seconds, t.Milliseconds));
                        powder.Add(new PowderToyComment(ID, Username[Username.Count - 1], CommentText[CommentText.Count - 1], Date[Date.Count - 1]));
                    }
                }
            return powder;
        }
        public void Alarm(int Elapsed)
        {
            timer.Interval = Elapsed;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);


            HttpWebRequest wReq;
            HttpWebResponse wRes;
            int CommentCount = 0;
            int count = CommentCount + 20;
            Uri uri = new Uri("http://powdertoy.co.uk/Browse/Comments.json?ID=" + ID + "& Start =" + CommentCount + "&Count=" + count); // string 을 Uri 로 형변환
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
            if (res == "Error: 404")
            {
                throw new Exception("Invalid ID");
            }
            JsonTextParser parser = new JsonTextParser();
            JsonObject obj = parser.Parse(res);
            JsonArrayCollection col = (JsonArrayCollection)obj;

            List<string> CommentText = new List<string>();
            List<string> Timestamp = new List<string>();
            foreach (JsonObjectCollection joc in col)
            {
                CommentText.Add((string)joc["Text"].GetValue());
                Timestamp.Add((string)joc["Timestamp"].GetValue());
                TimeSpan t = TimeSpan.FromSeconds(Convert.ToInt32(Timestamp[Timestamp.Count - 1]));
                int hour = t.Hours + 9;
                if (hour > 24)
                {
                    hour = hour - 24;
                    if (hour >= 12)
                        hour = hour + 12;
                }

            }
            전에 = CommentText.ToArray();

            timer.Start();
        }



        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            HttpWebRequest wReq;
            HttpWebResponse wRes;
            int CommentCount = 0;
            int count = CommentCount + 20;
            Uri uri = new Uri("http://powdertoy.co.uk/Browse/Comments.json?ID=" + ID + "& Start =" + ID + "&Count=" + count); // string으로 URI 생성
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
            JsonArrayCollection col = (JsonArrayCollection)obj;

            string[] Username = new string[22];
            string[] CommentText = new string[22];
            string[] Timestamp = new string[22];
            DateTime[] Date = new DateTime[22];
            int i = 0;
            foreach (JsonObjectCollection joc in col)
            {
                i++;
                Username[i] = (string)joc["Username"].GetValue();
                CommentText[i] = (string)joc["Text"].GetValue();
                Timestamp[i] = (string)joc["Timestamp"].GetValue();
                Console.WriteLine(Username[i] + CommentText[i] + Timestamp[i]);
                TimeSpan t = TimeSpan.FromSeconds(Convert.ToInt32(Timestamp[i]));
                int hour = t.Hours + 9;
                if (hour > 24)
                {
                    hour = hour - 24;
                    if (hour >= 12)
                        hour = hour + 12;
                }
                DateTime Date1 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + t;
                Date[i] = new DateTime(Date1.Year, Date1.Month, Date1.Day, hour, t.Minutes, t.Seconds, t.Milliseconds);

                Console.WriteLine(Username[i] + CommentText[i]);

            }

            if (전에[1] != CommentText[1])
            {
                전에 = CommentText;
                DoClick(new PowderToyComment(ID, Username[1], CommentText[1], Date[1]));
            }
        }


    }
}
