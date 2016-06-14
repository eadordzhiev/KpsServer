using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConsoleApplication1
{
    class Program
    {
        public class KpsData
        {
            public IList<Zone> Zones { get; set; }
        }
        public class Geoposition
        {
            public double Latitude { get; set; }

            public double Longitude { get; set; }
        }

        public class Zone
        {
            public string Name { get; set; }

            public IList<Geoposition> Positions { get; set; }

            public double ThreatLevel { get; set; }
        }

        static void Main(string[] args)
        {
            var json = File.ReadAllText(@"C:\Users\Desktop1\Desktop\a.json");
            var data = JsonConvert.DeserializeObject<KpsData>(json);

            var lod = 11;
            var lodDelta = 2;
            lod += lodDelta;
            var lodPower = (int) Math.Pow(2, lodDelta);
            var startXTile = 1236 * lodPower;
            var endXTile = 1240 * lodPower;
            var startYTile = 636 * lodPower;
            var endYTile = 643 * lodPower;
            int startY;
            int startX;
            TileSystem.TileXYToPixelXY(startXTile, startYTile, out startX, out startY);
            int endX;
            int endY;
            TileSystem.TileXYToPixelXY(endXTile, endYTile, out endX, out endY);
            var bitmap = new Bitmap(endX - startX, endY - startY);
            var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Transparent);

            foreach (var zone in data.Zones)
            {
                var points = new List<Point>();
                foreach (var geoposition in zone.Positions)
                {
                    long x;
                    long y;
                    TileSystem.LatLongToPixelXY(geoposition.Latitude, geoposition.Longitude, lod, out x, out y);

                    x -= startX;
                    y -= startY;

                    if (x < 0 || y < 0 || x >= bitmap.Width || y >= bitmap.Height)
                    {
                        points = null;
                        break;
                    }

                    points.Add(new Point((int) x, (int) y));
                }

                if (points == null)
                {
                    continue;
                }

                var threatLevel = zone.ThreatLevel;
                var color = threatLevel < 0.5
                    ? ColorInterpolator.InterpolateBetween(Color.LawnGreen, Color.Yellow, threatLevel * 2)
                    : ColorInterpolator.InterpolateBetween(Color.Yellow, Color.Red, (threatLevel - 0.5) * 2);
                color = Color.FromArgb(100, color);
                graphics.FillPolygon(new SolidBrush(color), points.ToArray());
            }

            graphics.Dispose();
            bitmap.Save(@"C:\Users\Desktop1\Desktop\out.png", ImageFormat.Png);
        }
    }
}
