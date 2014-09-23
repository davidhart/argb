using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;

namespace ARGB
{
    class Program
    {
        const int width = 512;
        const int height = 512;
        const int colordepth = 64;
        const int colordepthsquare = colordepth * colordepth;
        const int colordepthcube = colordepth * colordepth * colordepth;
        const int colorScale = 4;

        static BitArray visitedColors = new BitArray(colordepth * colordepth * colordepth);
        static BitArray visitedPixels = new BitArray(width * height);
        static HashSet<int> freePixels = new HashSet<int>();
        static HashSet<int> freeColors = new HashSet<int>();

        static Random random = new Random(0);

        static int MinX;
        static int MinY;
        static int MaxX;
        static int MaxY;

        static byte[] pixels = new byte[width * height * 3];

        static void Main(string[] args)
        {
            Stopwatch s = new Stopwatch();
            s.Start();

            PopulatePixels(pixels);

            SaveImage("final.png");

            s.Stop();
            Console.WriteLine(s.Elapsed);
        }

        static void SaveImage(string filename)
        {
            Bitmap bitmap = new Bitmap(width, height);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height),
                                              ImageLockMode.WriteOnly,
                                              PixelFormat.Format24bppRgb);

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);

            bitmap.UnlockBits(data);

            bitmap.Save(filename);
        }

        static void PopulatePixels(byte[] pixels)
        {
            int p = XYToInt(0, 1 * height / 4 - 1);
            //int p = random.Next(colordepthcube);
            int c = random.Next(colordepthcube);

            MinX = 0;
            MinY = 0;
            MaxX = 1;
            MaxX = 1;
            
            long work = 0;
            long prevWorkPercent = -1;
            long prevworkperdec = 0;

            while (work < colordepthcube)
            {
                c = GetNextColor(c);
                p = GetNextPixel(p, c);

                int x = XFromInt(p);
                int y = YFromInt(p);

                if (x - 1 > 0 && x - 1 < MinX)
                    MinX = x - 1;
                if (y - 1 > 0 && y - 1 < MinY)
                    MinY = y - 1;
                if (x + 1 < width && x + 1 > MaxX)
                    MaxX = x + 1;
                if (y + 1 < height && y + 1 > MaxY)
                    MaxY = y + 1;

                int index = p * 3;
                pixels[index] = (byte)(RFromInt(c) * colorScale);
                pixels[index + 1] = (byte)(GFromInt(c) * colorScale);
                pixels[index + 2] = (byte)(BFromInt(c) * colorScale);

                visitedColors.Set(c, true);
                visitedPixels.Set(p, true);

                work += 1;
                
                freePixels.Remove(p);
                UpdatePixel(x - 1, y);
                UpdatePixel(x + 1, y);
                UpdatePixel(x, y - 1);
                UpdatePixel(x, y + 1);

                freeColors.Remove(c);
                int r = RFromInt(c);
                int g = GFromInt(c);
                int b = BFromInt(c);
                UpdateColor(r + 1, g, b);
                UpdateColor(r - 1, g, b);
                UpdateColor(r, g + 1, b);
                UpdateColor(r, g - 1, b);
                UpdateColor(r, g, b + 1);
                UpdateColor(r, g, b - 1);
                
                long percent = work * 10000 / colordepthcube;

                if (percent != prevWorkPercent)
                {
                    Console.WriteLine(string.Format("{0:0.00}%", percent / 100.0f));
                    prevWorkPercent = percent;
                }

                long perdec = percent / 1000;

                if (perdec != prevworkperdec)
                {
                    Console.WriteLine("dumping");

                    SaveImage(string.Format("progress{0}.png", perdec));
                    prevworkperdec = perdec;
                }
            }

            bool allColorsOk = true;
            bool allPixelsOk = true;
            for (int i = 0; i < colordepthcube; ++i)
            {
                if (visitedColors.Get(i) == false) allColorsOk = false;
                if (visitedPixels.Get(i) == false) allPixelsOk = false;
            }

            if (allColorsOk == false)
                Console.WriteLine("Verification of colors failed!");

            if (allPixelsOk == false)
                Console.WriteLine("Verification of pixels failed!");

        }

        static void UpdateColor(int r, int g, int b)
        {
            int c = RGBToInt(r, g, b);

            if (IsColorOpen(r, g, b))
            {
                freeColors.Add(c);
            }
            else
            {
                freeColors.Remove(c);
            }
        }

        static bool IsColorOpen(int r, int g, int b)
        {
            if (r >= colordepth ||
                g >= colordepth ||
                b >= colordepth ||
                r < 0 ||
                g < 0 ||
                b < 0 )
                return false;

            return visitedColors.Get(RGBToInt(r, g, b)) == false;
        }

        static void UpdatePixel(int x, int y)
        {
            int p = XYToInt(x, y);

            if (IsPixelOpen(x, y))
            {
                freePixels.Add(p);
            }
            else
            {
                freePixels.Remove(p);
            }
        }

        static bool PixelSet(int x, int y)
        {
            if (x < 0) return false;
            if (x >= width) return false;
            if (y < 0) return false;
            if (y >= height) return false;

            return visitedPixels.Get(XYToInt(x, y));
        }

        static bool IsPixelOpen(int x, int y)
        {
            if (x < 0) return false;
            if (x >= width) return false;
            if (y < 0) return false;
            if (y >= height) return false;

            return visitedPixels.Get(XYToInt(x, y)) == false;
        }

        static int GetNextColor(int c)
        {
            int r = RFromInt(c);
            int g = GFromInt(c);
            int b = BFromInt(c);

            int colordirection = random.Next(6);

            switch (colordirection)
            {
                case 0:
                    r++;
                    break;
                case 1:
                    r--;
                    break;
                case 2:
                    g++;
                    break;
                case 3:
                    g--;
                    break;
                case 4:
                    b++;
                    break;
                case 5:
                    b--;
                    break;
            }

            if (r < 0) r = 0;
            if (r >= colordepth) r = colordepth - 1;
            if (g < 0) g = 0;
            if (g >= colordepth) g = colordepth - 1;
            if (b < 0) b = 0;
            if (b >= colordepth) b = colordepth - 1;

            c = RGBToInt(r, g, b);

            if (visitedColors.Get(c))
            {
                c = NearestBestColor(c);
            }

            return c;
        }

        static int NearestBestColor(int c)
        {
            int bestC = 0;
            int distance = colordepthsquare;

            //while(NearestBestColorManhattanDistance(c, out bestC, distance++) == false);

            int r0 = RFromInt(c);
            int g0 = GFromInt(c);
            int b0 = BFromInt(c);


            foreach (int newc in freeColors)
            {
                int r1 = RFromInt(newc);
                int g1 = GFromInt(newc);
                int b1 = BFromInt(newc);

                int d = Math.Abs(r1 - r0) + Math.Abs(g1 - g0) + Math.Abs(b1 - b0);
                if (d < distance)
                {
                    distance = d;
                    bestC = newc;
                }

                if (d == 1)
                    break;
            }

            return bestC;
        }

        static List<int> BestColors = new List<int>();

        static bool NearestBestColorManhattanDistance(int c, out int best, int distance)
        {
            best = 0;

            int targetR = RFromInt(c);
            int targetG = GFromInt(c);
            int targetB = BFromInt(c);

            BestColors.Clear();

            for (int r = 0; r <= distance; ++r)
            {
                for (int g = 0; g <= distance - r; ++g)
                {
                    int b = distance - r - g;

                    CollectNeighbourColors(targetR, targetG, targetB, r, g, b);
                }
            }

            if (BestColors.Count == 0)
                return false;

            int index = random.Next(0, BestColors.Count);

            best = BestColors[index];
            return true;
        }
        
        static int[] lutR = { 1,  1,  1,  1, -1, -1, -1, -1 };
        static int[] lutG = { 1,  1, -1, -1,  1,  1, -1, -1 };
        static int[] lutB = { 1, -1,  1, -1,  1, -1,  1, -1 };

        static void CollectNeighbourColors(int targetR, int targetG, int targetB, int r, int g, int b)
        {
            for (int i = 0; i < 8; ++i)
            {
                int newR = targetR + r * lutR[i];

                if (newR >= colordepth)
                    continue;
                if (newR < 0)
                    continue;

                int newG = targetG + g * lutG[i];

                if (newG >= colordepth)
                    continue;
                if (newG < 0)
                    continue;

                int newB = targetB + b * lutB[i];

                if (newB >= colordepth)
                    continue;
                if (newB < 0)
                    continue;

                int newC = RGBToInt(newR, newG, newB);

                if (visitedColors.Get(newC) == false && BestColors.Contains(newC) == false)
                {
                    BestColors.Add(newC);
                }
            }
        }

        static double[] randweights = new double[4];

        static int GetNextPixel(int p, int c)
        {
            int x = XFromInt(p);
            int y = YFromInt(p);

            double xd = width / 2.0 - x;
            double yd = height / 2.0 - y;

            double len = Math.Sqrt(xd * xd + yd * yd);
            xd /= len;
            yd /= len;

            double temp = xd;
            xd = -yd;
            yd = temp;

            double xda = Math.Abs(xd);
            double yda = Math.Abs(yd);

            randweights[0] = xd > 0 ? xda / (xda + yda) : 0;
            randweights[1] = xd <= 0 ? xda / (xda + yda) : 0;
            randweights[2] = yd > 0 ? yda / (xda + yda) : 0;
            randweights[3] = yd <= 0 ? yda / (xda + yda) : 0;

            if (randweights[0] + randweights[1] + randweights[2] + randweights[3] < 0.99)
            {
                Console.WriteLine("WOT");
            }
            
            double rand = random.NextDouble();
            
            double f = 0;
            int i = 0;
            while ( (f += randweights[i++]) < rand && i < 4) {}

            if (f > 1.0f)
                i = 0;

            int pixeldirection = i;


            switch (pixeldirection)
            {
            case 0:
                x++;
                break;
            case 1:
                x--;
                break;
            case 2:
                y++;
                break;
            case 3:
                y--;
                break;
            }

            if (x < 0) x = 0;
            if (x >= width) x = width - 1;
            if (y < 0) y = 0;
            if (y >= height) y = height - 1;

            p = XYToInt(x, y);

            if (visitedPixels.Get(p))
            {
                p = NearestBestPixel(p, c);
            }

            return p;
        }

        static int NearestBestPixel(int p, int c)
        {
            //int bestP = 0;
            //int distance = 1;

            //while (NearestBestPixelManhattanDistance(p, out bestP, distance++) == false);
            
            int bestP = 0;
            int bestDelta = colordepth * 3;

            int r = RFromInt(c);
            int g = GFromInt(c);
            int b = BFromInt(c);

            foreach (int freePixel in freePixels)
            {
                int x = XFromInt(freePixel);
                int y = YFromInt(freePixel);

                int neighbour = XYToInt(x - 1, y);

                if (x > 0 && visitedPixels.Get(neighbour) == true)
                {
                    int delta = ColorDelta(neighbour, r, g, b);

                    if (delta < bestDelta)
                    {
                        bestDelta = delta;
                        bestP = freePixel;
                    }
                }

                neighbour = XYToInt(x + 1, y);
                if (x < width - 1 && visitedPixels.Get(neighbour) == true)
                {
                    int delta = ColorDelta(neighbour, r, g, b);

                    if (delta < bestDelta)
                    {
                        bestDelta = delta;
                        bestP = freePixel;
                    }
                }

                neighbour = XYToInt(x, y - 1);
                if (y > 0 && visitedPixels.Get(neighbour) == true)
                {
                    int delta = ColorDelta(neighbour, r, g, b);

                    if (delta < bestDelta)
                    {
                        bestDelta = delta;
                        bestP = freePixel;
                    }
                }

                neighbour = XYToInt(x, y + 1);
                if (y < height - 1 && visitedPixels.Get(neighbour) == true)
                {
                    int delta = ColorDelta(neighbour, r, g, b);

                    if (delta < bestDelta)
                    {
                        bestDelta = delta;
                        bestP = freePixel;
                    }
                }

                if (bestDelta == 1)
                    break;
            }

            return bestP;
        }

        static List<int> BestPixels = new List<int>();

        static int ColorDelta(int p, int r, int g, int b)
        {
            int r1 = pixels[p * 3] / colorScale;
            int g1 = pixels[p * 3 + 1] / colorScale;
            int b1 = pixels[p * 3 + 2] / colorScale;

            return Math.Abs(r - r1) + Math.Abs(g - g1) + Math.Abs(b - b1);
        }


        static bool NearestBestPixelManhattanDistance(int p, out int bestP, int distance)
        {
            bestP = 0;
            BestPixels.Clear();

            int targetX = XFromInt(p);
            int targetY = YFromInt(p);

            for (int x = 0; x <= distance; ++x)
            {
                int y = distance - x;

                CollectNeighbourPixels(targetX, targetY, x, y);
            }

            if ( BestPixels.Count == 0 )
                return false;

            int index = random.Next(0, BestPixels.Count);

            bestP = BestPixels[index];

            return true;
        }

        static int[] lutX = { 1, 1, -1, -1 };
        static int[] lutY = { 1, -1, 1, -1 };

        static void CollectNeighbourPixels(int targetX, int targetY, int x, int y)
        {
            for (int i = 0; i < 4; ++i)
            {
                int newX = targetX + x * lutX[i];

                if (newX >= width)
                    continue;
                if (newX < 0)
                    continue;

                int newY = targetY + y * lutY[i];

                if (newY >= height)
                    continue;
                if (newY < 0)
                    continue;

                int newP = XYToInt(newX, newY);

                if (visitedPixels.Get(newP) == false && BestPixels.Contains(newP) == false)
                {
                    BestPixels.Add(newP);
                }
            }
        }

        static int XFromInt(int p)
        {
            return p % width;
        }

        static int YFromInt(int p)
        {
            return p / width;
        }

        static int XYToInt(int x, int y)
        {
            return x + y * width;
        }

        static int RGBToInt(int r, int g, int b)
        {
            return r + g * colordepth + b * colordepthsquare;
        }

        static byte RFromInt(int c)
        {
            return (byte)(c % colordepth);
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
