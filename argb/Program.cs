using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;
using System;


namespace ARGB
{
    class Program
    {
        const int width = 512;
        const int height = 512;
        const int colordepth = 64;
        const int colordepthsquare = colordepth * colordepth;
        const int colordepthcube = colordepth * colordepth * colordepth;

        static Random random = new Random(0);

        static void Main(string[] args)
        {
            
            byte[] pixels = new byte[width * height * 3];

            PopulatePixels(pixels);

            Bitmap bitmap = new Bitmap(width, height);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height),
                                              ImageLockMode.WriteOnly,
                                              PixelFormat.Format24bppRgb);

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);

            bitmap.UnlockBits(data);

            bitmap.Save("dump.png");
        }

        static void PopulatePixels(byte[] pixels)
        {
            BitArray visitedColors = new BitArray(colordepth * colordepth * colordepth);
            BitArray visitedPixels = new BitArray(width * height);

            int p = random.Next(colordepthcube);
            int c = random.Next(colordepthcube);

            int work = 0;
            int prevWorkPercent = -1;

            while (work < colordepthcube)
            {
                do
                {
                    int colordirection = random.Next(6);
                    switch (colordirection)
                    {
                        case 0:
                            c += 1;
                            break;
                        case 1:
                            c -= 1;
                            break;
                        case 2:
                            c += colordepth;
                            break;
                        case 3:
                            c -= colordepth;
                            break;
                        case 4:
                            c += colordepthsquare;
                            break;
                        case 5:
                            c -= colordepthsquare;
                            break;
                    }

                    if (c < 0)
                        c = colordepthcube + c;

                    c = c % colordepthcube;

                } while (visitedColors.Get(c));

                do
                {
                    int pixeldirection = random.Next(4);
                    switch (pixeldirection)
                    {
                        case 0:
                            p += 1;
                            break;
                        case 1:
                            p -= 1;
                            break;
                        case 2:
                            p += width;
                            break;
                        case 3:
                            p -= width;
                            break;
                    }

                    if (p < 0)
                        p = width * height + p;

                    p = p % (width * height);

                } while (visitedPixels.Get(p));

                int index = p * 3;
                pixels[index] = (byte)(RFromInt(c) * 4);
                pixels[index + 1] = (byte)(GFromInt(c)*4);
                pixels[index + 2] = (byte)(BFromInt(c)*4);

                visitedColors.Set(c, true);
                visitedPixels.Set(p, true);

                work += 1;

                int percent = work * 100 / colordepthcube;

                if (percent != prevWorkPercent)
                {
                    Console.WriteLine(percent + "% done");
                    prevWorkPercent = percent;
                }
            }
        }

        static int XFromInt(int p)
        {
            return p % height;
        }

        static int YFromInt(int p)
        {
            return p / height;
        }

        static byte RFromInt(int c)
        {
            return (byte)(c % colordepthsquare);
        }

        static byte GFromInt(int c)
        {
            return (byte)((c / colordepth) % colordepth);
        }

        static byte BFromInt(int c)
        {
            return (byte)(c / colordepthsquare);
        }
    }

}
