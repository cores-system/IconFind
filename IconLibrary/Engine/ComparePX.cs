using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace IconLibrary.Engine
{
    public static class ComparePX
    {
        public static int Compare(List<bool> searchHash, Bitmap icon)
        {
            List<bool> hash = GetHash(icon);

            //determine the number of equal pixel (x of (size*size))
            return hash.Zip(searchHash, (i, j) => i == j).Count(eq => eq);
        }
        

        #region GetHash
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmpSource"></param>
        public static List<bool> GetHash(Bitmap bmpSource)
        {
            List<bool> lResult = new List<bool>();

            Bitmap bmpMin = new Bitmap(IMG.Transparent2Color(bmpSource, Color.White), new Size(32, 32));
            for (int j = 0; j < bmpMin.Height; j++)
            {
                for (int i = 0; i < bmpMin.Width; i++)
                {
                    //reduce colors to true / false                
                    lResult.Add(bmpMin.GetPixel(i, j).GetBrightness() < 0.5f);
                }
            }
            return lResult;
        }
        #endregion
    }
}
