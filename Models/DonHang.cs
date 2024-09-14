using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TapHoa.Models
{
    public class DonHang
    {
        public string IdSanPham { get; set; }
        public string TenSanPham { get; set; }
        public int SoLuong { get; set; }
        public decimal Gia { get; set; }

        public DonHang() { }

        public DonHang(string idSanPham, int soLuong, decimal gia)
        {
            IdSanPham = idSanPham;
            SoLuong = soLuong;
            Gia = gia;
        }

        public decimal FinalPrice()
        {
            return SoLuong * Gia;
        }
    }


}