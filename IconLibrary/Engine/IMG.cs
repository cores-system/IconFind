using ImageMagick;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace IconLibrary.Engine
{
    public static class IMG
    {
        #region ImageInfo
        public static (int Width, int Height) ImageInfo(string fileImg)
        {
            using (var fs = File.OpenRead(fileImg))
            {
                byte[] header = new byte[8];
                fs.Read(header, 0, 8);
                if (!header.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
                    throw new InvalidOperationException("not a png file");

                var br = new BinaryReader(fs, Encoding.ASCII, true);
                while (fs.Position != fs.Length)
                {
                    uint chunkLength = br.ReadUInt32BE();
                    string chunkType = new string(br.ReadChars(4));
                    if (chunkType == "IHDR")
                    {
                        // читаем чанк
                        uint width = br.ReadUInt32BE();
                        uint height = br.ReadUInt32BE();
                        //byte bitDepth = br.ReadByte();
                        //byte colorType = br.ReadByte();
                        //byte comprMethod = br.ReadByte();
                        //byte filterMethod = br.ReadByte();
                        //byte interlaceMethod = br.ReadByte();
                        return ((int)width, (int)height);
                    }
                    else
                        // пропускаем чанк
                        fs.Position += chunkLength;
                    uint crc = br.ReadUInt32BE();
                }
            }

            return (0, 0);
        }
        #endregion

        #region Transparent2Color
        /// <summary>
        /// Прозрачный в указаный
        /// </summary>
        public static Bitmap Transparent2Color(Bitmap bmp1, Color target)
        {
            Bitmap bmp2 = new Bitmap(bmp1.Width, bmp1.Height);
            Rectangle rect = new Rectangle(Point.Empty, bmp1.Size);
            using (Graphics G = Graphics.FromImage(bmp2))
            {
                G.Clear(target);
                G.DrawImageUnscaledAndClipped(bmp1, rect);
            }
            return bmp2;
        }
        #endregion

        #region MeasureImageCrop
        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Rectangle MeasureImageCrop(Bitmap image)
        {
            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            var bmData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            var stride = bmData.Stride;
            var scan0 = bmData.Scan0;

            var cutLeft = 0;
            var cutRight = 0;
            var cutTop = 0;
            var cutBottom = 0;

            const int step = 1;

            unsafe
            {
                var p = (byte*)(void*)scan0;

                var refBlue = p[0];
                var refGreen = p[1];
                var refRed = p[2];
                var refAlfa = p[3];

                // --
                // Top.

                var wantBreak = false;
                for (var y = 0; y < image.Height; y += step)
                {
                    p = ((byte*)(void*)scan0) + stride * y;
                    for (var x = 0; x < image.Width; x += step)
                    {
                        var blue = p[0];
                        var green = p[1];
                        var red = p[2];
                        var alfa = p[3];

                        if (alfa == 255)
                        {
                            wantBreak = true;
                            break;
                        }

                        p += 4 * step;
                    }

                    if (wantBreak)
                        break;

                    cutTop += step;
                }

                // 
                if (cutTop > 0)
                    cutTop -= 1;

                // --
                // Bottom.

                wantBreak = false;
                for (var y = image.Height - 1; y >= 0; y -= step)
                {
                    p = ((byte*)(void*)scan0) + stride * y;

                    for (var x = 0; x < image.Width; x += step)
                    {
                        var blue = p[0];
                        var green = p[1];
                        var red = p[2];
                        var alfa = p[3];

                        if (alfa == 255)
                        {
                            wantBreak = true;
                            break;
                        }

                        p += 4 * step;
                    }

                    if (wantBreak)
                    {
                        break;
                    }

                    cutBottom += step;
                }

                //
                if (cutBottom > 0)
                    cutBottom -= 1;

                // --
                // Left.

                p = (byte*)(void*)scan0;

                wantBreak = false;
                for (var x = 0; x < image.Width; x += step)
                {
                    for (var y = 0; y < image.Height; y += step)
                    {
                        var blue = p[0];
                        var green = p[1];
                        var red = p[2];
                        var alfa = p[3];

                        if (alfa == 255)
                        {
                            wantBreak = true;
                            break;
                        }

                        p += stride * step;
                    }

                    if (wantBreak)
                    {
                        break;
                    }

                    cutLeft += step;

                    p -= stride * image.Height;
                    p += 4 * step;
                }

                //
                if (cutLeft > 0)
                    cutLeft -= 1;

                // --
                // Right.

                p = (byte*)(void*)scan0;

                p += (image.Width - 1) * 4;

                wantBreak = false;
                for (var x = image.Width - 1; x >= 0; x -= step)
                {
                    for (var y = 0; y < image.Height; y += step)
                    {
                        var blue = p[0];
                        var green = p[1];
                        var red = p[2];
                        var alfa = p[3];

                        if (alfa == 255)
                        {
                            wantBreak = true;
                            break;
                        }

                        p += stride * step;
                    }

                    if (wantBreak)
                    {
                        break;
                    }

                    cutRight += step;

                    p -= stride * image.Height;
                    p -= 4 * step;
                }
            }

            //
            if (cutRight > 0)
                cutRight -= 1;

            // --

            image.UnlockBits(bmData);

            //
            int Width = image.Width - cutRight - cutLeft;
            int Height = image.Height - cutBottom - cutTop;

            return new Rectangle(cutLeft, cutTop, Width, Height);

            //if (Height > Width)
            //{
            //    return new Rectangle(
            //    cutLeft - ((Height - Width) / 2),
            //    cutTop,
            //    Width + (Height - Width),
            //    Height);
            //}
            //else if (Height == Width)
            //{
            //    return new Rectangle(cutLeft, cutTop, Width, Height);
            //}
            //else
            //{
            //    return new Rectangle(
            //    cutLeft,
            //    cutTop - ((Width - Height) / 2),
            //    Width,
            //    Height + (Width - Height));
            //}
        }
        #endregion

        #region CropImage
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static Bitmap CropImage(Bitmap source, Rectangle section)
        {
            // An empty bitmap which will hold the cropped image
            Bitmap bmp = new Bitmap(section.Width, section.Height);

            Graphics g = Graphics.FromImage(bmp);

            // Draw the given area (section) of the source image
            // at location 0,0 on the empty bitmap (bmp)
            g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);

            return bmp;
        }
        #endregion

        #region resizeToCanvas
        public static Bitmap resizeToCanvas(Bitmap img, int width, int height)
        {
            MagickImage image = new MagickImage(img);
            image.Resize(width, height);

            using (var src = image.ToBitmap())
            {
                using (var bmp = new Bitmap(width, height))
                {
                    using (var gr = Graphics.FromImage(bmp))
                    {
                        gr.DrawImage(src, (width - src.Width) / 2, (height - src.Height) / 2);
                        gr.DrawImage(src, (width - src.Width) / 2, (height - src.Height) / 2);
                        return new Bitmap(bmp, width, height);
                    }
                }
            }
        }
        #endregion

        #region ColorToBlack
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmp"></param>
        public static Bitmap ColorToBlack(Bitmap bmp)
        {
            float factor = 0.001f;
            Bitmap res = new Bitmap(bmp.Width, bmp.Height);

            for (int x = 0; x < bmp.Width; ++x)
            {
                for (int y = 0; y < bmp.Height; ++y)
                {
                    Color curr = bmp.GetPixel(x, y);

                    // Прозрачный
                    if (curr.A == 0)
                        continue;

                    Color next = Color.FromArgb((byte)(factor * curr.R), (byte)(factor * curr.G), (byte)(factor * curr.B));
                    res.SetPixel(x, y, next);
                }
            }

            return res;
        }
        #endregion

        #region ColorToWhite
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmp"></param>
        public static Bitmap ColorToWhite(Bitmap bmp)
        {
            MagickImage image = new MagickImage(bmp);
            image.BrightnessContrast(new Percentage(100), new Percentage(0), Channels.RGB);
            return image.ToBitmap();
        }
        #endregion

        #region ConvertIconsToCv
        public static void ConvertIconsToCv(string locationPath)
        {
            foreach (var iconFile in Directory.GetFiles(locationPath + @"\icons"))
            {
                // Уже есть сконвертированя иконка
                if (File.Exists(iconFile.Replace(@"\icons\", @"\icons_cv\")))
                    continue;

                // Открываем иконку
                Bitmap bmp = new Bitmap(iconFile);

                // Обрезаем пустые границы изображений
                var rt = MeasureImageCrop(bmp);
                if (rt.Width > 0 && rt.Height > 0 && rt.Width != bmp.Width && rt.Height != bmp.Height)
                {
                    // Делаем ресайз
                    var image = resizeToCanvas(CropImage(bmp, rt), 64, 64);

                    // Сохраняем иконку
                    image.Save(iconFile.Replace(@"\icons\", @"\icons_cv\"));
                }
                else
                {
                    // Сохраняем оригинал
                    bmp.Save(iconFile.Replace(@"\icons\", @"\icons_cv\"));
                }
            }
        }
        #endregion
    }
}
