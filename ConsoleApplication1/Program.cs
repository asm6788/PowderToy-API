using PowderToyApi;
using System;
using System.Collections.Generic;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {

            //PowderToy.Auth("ID", "PW");
            //PowderToySaveStatus powsave = new PowderToySaveStatus(1);
            //powsave = powsave;
            //PowderToyComment powe = new PowderToyComment(0);
            //powe.Alarm(10);
            //List<PowderToyComment> pow = new List<PowderToyComment>();
            //pow = powe.GetAll();
            //pow = pow;
            //powe.Newcomment += Powe_Newcomment;
            //PowderToy.AddComment(0, "lolololololololololollololol");
            Console.Read();
        }

        private static void Powe_Newcomment(object sender, PowderToyComment e)
        {
            Console.WriteLine("새댓글~!!!: " + e.Username + e.Comment + e.Time);
        }
    }
}
