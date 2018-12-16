using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace IconLibrary.Engine
{
    public class ComparePercent
    {
        public static double Compare(IDictionary<int, List<byte>> searchHash, Bitmap icon)
        {
            List<int> math = new List<int>();

            // 
            var iconHash = GetHash(icon);

            // Картинка size x size
            // Проходим каждую линию по высоте
            for (var l = 0; l < 64; l++)
            {
                math.Add(0);

                // Вытаскиваем пиксели линии двух картинок
                var pixelsCompare = searchHash[l];
                var pixelsImage = iconHash[l];

                // Проходиммся по пикселям
                for (var i = 0; i < pixelsImage.Count; i++)
                {
                    //если альфа каналы равны, то в math плюсуем
                    if (pixelsCompare[i] == pixelsImage[i])
                        math[l]++;
                }
            }

            // Вытаскиваем среднее число
            var bet = math.Average();

            // Процент совпадения
            return (bet / 64 * 100);
        }

        #region GetHash
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static IDictionary<int, List<byte>> GetHash(Bitmap bmp)
        {
            IDictionary<int, List<byte>> result = new Dictionary<int, List<byte>>();

            for (int y = 0; y < bmp.Height; y++)
            {
                List<byte> lResult = new List<byte>();
                for (int x = 0; x < bmp.Width; x++)
                {
                    lResult.Add(bmp.GetPixel(x, y).A);
                }

                result.Add(y, lResult);
            }
            
            return result;
        }
        #endregion
    }
}
