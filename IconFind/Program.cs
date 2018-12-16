using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using IconLibrary;

namespace IconFind
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var md = IconCompare.Compare(int.Parse(args[0]), args[1], Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            Console.WriteLine(JsonConvert.SerializeObject(md));
        }
    }
}
