using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

namespace KpsServer.Controllers
{
    public class MapController : ApiController
    {
        static Image map = Image.FromFile(HostingEnvironment.MapPath("~/App_Data/out.png"));

        public HttpResponseMessage Get(int zoomLevel, int x, int y)
        {
            var lod = 11;
            var lodDelta = 2;
            lod += lodDelta;
            var lodPower = (int)Math.Pow(2, lodDelta);
            var startXTile = 1236 * lodPower;
            var startYTile = 636 * lodPower;
            int startY;
            int startX;
            TileSystem.TileXYToPixelXY(startXTile, startYTile, out startX, out startY);

            var x1 = x * 256 * Math.Pow(2, lod - zoomLevel);
            var y1 = y * 256 * Math.Pow(2, lod - zoomLevel);
            var w = 256 * Math.Pow(2, lod - zoomLevel);
            
            var bitmap = new Bitmap(256, 256);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                lock (map)
                {
                    graphics.DrawImage(map, new Rectangle(0, 0, 256, 256), (float) (x1 - startX), (float)(y1 - startY), (float)w, (float)w, GraphicsUnit.Pixel);
                }
            }

            var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            stream.Position = 0;

            var response = new HttpResponseMessage();
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
            return response;
        }
    }
}
