using PowderToyApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //PowderToy.Auth("ID","PW");
            PowderToyComment powe = new PowderToyComment(2030698);
            List<PowderToyComment> pow = new List<PowderToyComment>();
            pow = powe.GetAll();
            pow = pow;
            Console.Read();
        }
    }
}
