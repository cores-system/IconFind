using IconLibrary.Engine;
using IconLibrary.Models;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IconLibrary
{
    public static class IconCompare
    {
        public static CompareModel Compare(int take, string compareFile, string locationPath)
        {
            //
            IMG.ConvertIconsToCv(locationPath);

            // Искомая иконка
            Bitmap CompareImg = new Bitmap(compareFile);

            // Модель
            CompareModel md = new CompareModel();

            // Временные масивы
            ConcurrentBag<(string, int)> massPX = new ConcurrentBag<(string, int)>();
            ConcurrentBag<(string, double)> massPercent = new ConcurrentBag<(string, double)>();
            ConcurrentBag<(string, double)> massPxPercent = new ConcurrentBag<(string, double)>();


            // Хеши картинок
            var searchHashToPX = ComparePX.GetHash(IMG.resizeToCanvas(IMG.ColorToBlack(CompareImg), 32, 32));
            var searchHashToPercent = ComparePercent.GetHash(IMG.resizeToCanvas(CompareImg, 64, 64));
            var searchHashToPxPercent = ComparePxPercent.GetHash(IMG.resizeToCanvas(CompareImg, 64, 64));


            // Проходим по списку иконок
            Parallel.ForEach(Directory.GetFiles(locationPath + @"\icons_cv"), (iconFile) =>
            {
                // Открываем иконку
                using (Bitmap icon = new Bitmap(iconFile))
                {
                    // Сравниваем иконку с оригиналом
                    massPercent.Add((iconFile, ComparePercent.Compare(searchHashToPercent, icon)));
                    massPX.Add((iconFile, ComparePX.Compare(searchHashToPX, icon)));
                    massPxPercent.Add((iconFile, ComparePxPercent.Compare(searchHashToPxPercent, icon)));
                }
            });

            #region Сортируем найденые иконки
            foreach (var item in massPX.OrderByDescending(i => i.Item2).Take(take))
            {
                md.IconsToPX.Add(new FindIconToPX()
                {
                    Icon = Path.GetFileName(item.Item1).Replace(".png", ""),
                    NumberOfCoincidences = item.Item2
                });
            }
            
            foreach (var item in massPercent.OrderByDescending(i => i.Item2).Take(take))
            {
                md.IconsToPercent.Add(new FindIconToPercent()
                {
                    Icon = Path.GetFileName(item.Item1).Replace(".png", ""),
                    Percent = item.Item2
                });
            }
            
            foreach (var item in massPxPercent.OrderByDescending(i => i.Item2).Take(take))
            {
                md.IconsToPxPercent.Add(new FindIconToPxPercent()
                {
                    Icon = Path.GetFileName(item.Item1).Replace(".png", ""),
                    Percent = item.Item2
                });
            }
            #endregion


            // Успех
            return md;
        }
    }
}
