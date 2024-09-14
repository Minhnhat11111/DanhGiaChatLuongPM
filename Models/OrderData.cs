using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TapHoa.Models
{
    public class OrderData
    {
        public List<DonHang> SanPhams { get; set; }
        public decimal PhaiTra { get; set; }
        public decimal TienTraLai { get; set; }
    }

}