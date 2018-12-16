using IconLibrary.Models;
using IconLibrary.Types;
using ImageMagick;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IconLibrary.Engine
{
    public static class ComparePxPercent
    {
        public static double Compare(IDictionary<int, List<IconHash>> compareHash, Bitmap iconSource)
        {
            // Средний показатель
            List<double> GlobalAverage = new List<double>();

            // 
            var iconHash = GetHash(iconSource);

            // 
            for (int x = 0; x < compareHash.Count; x++)
            {
                List<double> LocalAverage = new List<double>();
                var compare = compareHash[x];
                var icon = iconHash[x];

                // 
                for (int y = 0; y < compare.Count; y++)
                {
                    if (y >= icon.Count) {
                        LocalAverage.Add(0);
                        break;
                    }

                    var itemToCompare = compare[y];
                    var itemToIcon = icon[y];

                    if (itemToCompare.IsBlack == itemToIcon.IsBlack)
                    {
                        LocalAverage.Add(Persent(itemToCompare.count, itemToIcon.count));
                    }
                    else
                    {
                        LocalAverage.Add(0);
                        break;
                    }
                }

                GlobalAverage.Add(LocalAverage.Average());
            }

            // Успех
            return GlobalAverage.Average();
        }


        #region GetHash
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmpSource"></param>
        public static IDictionary<int, List<IconHash>> GetHash(Bitmap bmpSource)
        {
            var hashs = new Dictionary<int, List<IconHash>>();

            for (int y = 0; y < bmpSource.Height; ++y)
            {
                #region Переменные
                bool LastPxToBlack = false, IsFirstPx = true;
                int value = 0;

                // 
                List<IconHash> xResult = new List<IconHash>();
                #endregion

                #region Локальный метод - "UpdateXResult"
                void UpdateXResult()
                {
                    if (LastPxToBlack)
                    {
                        xResult.Add(new IconHash()
                        {
                            IsBlack = true,
                            count = value
                        });
                    }
                    else
                    {
                        xResult.Add(new IconHash()
                        {
                            IsBlack = false,
                            count = value
                        });
                    }
                }
                #endregion

                #region Считаем количиство линий
                for (int x = 0; x < bmpSource.Width; ++x)
                {
                    Color curr = bmpSource.GetPixel(x, y);

                    if (IsFirstPx)
                    {
                        LastPxToBlack = curr.A > 0;
                        IsFirstPx = false;
                    }

                    // Черный
                    if (curr.A > 0)
                    {
                        if (!LastPxToBlack)
                        {
                            UpdateXResult();
                            LastPxToBlack = true;
                            value = 0;
                        }
                    }
                    else
                    {
                        if (LastPxToBlack)
                        {
                            UpdateXResult();
                            LastPxToBlack = false;
                            value = 0;
                        }
                    }

                    value++;
                }
                #endregion

                #region Вносим последний результат в xResult
                if (xResult.Count == 0)
                {
                    UpdateXResult();
                }
                else
                {
                    var item = xResult.Last();
                    if (LastPxToBlack && item.IsBlack)
                    {
                        item.count = value;
                    }
                    else
                    {
                        UpdateXResult();
                    }
                }
                #endregion

                // Добовляем хеш
                hashs.Add(y, xResult.Where(i => i.count > 2).ToList());
            }

            // Модель
            return hashs;
        }
        #endregion

        #region Persent
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        static double Persent(double val1, double val2)
        {
            if (val1 == val2)
                return 100;

            if (val1 == 0)
                val1 = 1;

            if (val2 == 0)
                val2 = 1;

            if (val1 > val2)
            {
                double number = val2;
                double persent = val1;
                return (number / persent) * 100;
            }
            else
            {
                double number = val1;
                double persent = val2;
                return (number / persent) * 100;
            }
        }
        #endregion
    }
}
