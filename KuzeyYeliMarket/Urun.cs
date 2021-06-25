using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KuzeyYeliMarket
{
    public class Urun
    {
        public int Id { get; set; }
        public int KategoriId { get; set; }
        public string UrunAd { get; set; }
        public decimal BirimFiyat { get; set; }
        public int StokAdet { get; set; }
        public byte[] Resim { get; set; }

        public Image ResmiGetir()
        {
            if (Resim == null) return null;
            return (Bitmap)((new ImageConverter()).ConvertFrom(Resim));
            //using (var ms = new MemoryStream(Resim))
            //{
            //    return Image.FromStream(ms);
            //}
        }
    }
}
