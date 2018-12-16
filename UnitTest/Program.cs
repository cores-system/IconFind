using IconLibrary;
using System;
using System.Linq;

namespace UnitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            FindIcon("bank", "university", 0);
            FindIcon("map", "map-marker-alt", 10);
            FindIcon("map2", "map-marker-alt", 10);
            FindIcon("lup", 0, new string[] { "search", "search-plus", "search-minus" });
            FindIcon("cloudversify", "cloudversify", 0);
            FindIcon("address-card", "address-card", 0);
            FindIcon("arrow-alt-circle-down", "arrow-alt-circle-down", 0);
            FindIcon("caret-right", "caret-right", 0);
            FindIcon("cloud-upload-alt", "cloud-upload-alt", 0);

            Console.WriteLine("\ndone");
            Console.ReadLine();
        }


        #region FindIcon
        static void FindIcon(string imgName, string imgFind, int targetIndex)
        {
            FindIcon(imgName, targetIndex, new string[] { imgFind });
        }

        static void FindIcon(string imgName, int targetIndex, string[] imgFind)
        {
            // Пути
            string locationPath = @"C:\Users\htc\Documents\Visual Studio 2017\Projects\IconFind\IconFind\bin\Release\";
            string iconsTest = @"C:\Users\htc\Documents\Visual Studio 2017\Projects\IconFind\UnitTest\bin\Release\icons\";

            // Поиск и вывод
            var md = IconCompare.Compare(300, $@"{iconsTest}\{imgName}.png", locationPath);
            int index = md.IconsToPxPercent.FindIndex(i => imgFind.Contains(i.Icon));
            Console.WriteLine($"{imgName}:\t" + (index <= targetIndex) + "\t| " + index);

            //foreach (var item in md.IconsToPxPercent.Take(15))
            //{
            //    Console.WriteLine(item.Icon + " / " + item.Percent);
            //}
        }
        #endregion
    }
}
