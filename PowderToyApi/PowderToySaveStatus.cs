﻿using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Json;
using System.Text;

namespace PowderToyApi
{
    public class PowderToySaveStatus
    {
        public string Username = "";
        public string Comment = "";
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
        public int totalpage = 0;
        public int CommentCount = 0;
        public Image SavefilePic = null;
        public PowderToySaveStatus(int ID)
        {
            this.ID = ID;
            HttpWebRequest wReq;
            HttpWebResponse wRes;

            WebRequest requestPic = WebRequest.Create("http://static.powdertoy.co.uk/" + ID + ".png");

            WebResponse responsePic = requestPic.GetResponse();
            SavefilePic = Image.FromStream(responsePic.GetResponseStream());
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

            JsonTextParser parser = new JsonTextParser();
            JsonObject obj = parser.Parse(res);

            JsonUtility.GenerateIndentedJsonText = false;


            int i = 0;
            string[] View = new string[17];
            foreach (JsonObject field in obj as JsonObjectCollection)
            {
                i++;
                string type = field.GetValue().GetType().Name;

                // try to get value.
                switch (type)
                {
                    case "String":
                        View[i] = (string)field.GetValue();
                        break;

                    case "Double":
                        View[i] = field.GetValue().ToString();
                        break;

                    case "Boolean":
                        View[i] = field.GetValue().ToString();
                        break;

                    default:
                        View[i] = null;
                        break;
                }

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

            Time = new TimeSpan(hour, t.Minutes, t.Seconds);
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
    }
}
