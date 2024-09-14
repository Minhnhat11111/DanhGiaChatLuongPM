using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace TapHoa.Models
{
    public class TruyVan
    {
        public static IEnumerable<SANPHAM> LaySanPham()
        {
            using (var db = new TapHoaEntities())
            {
                return db.SANPHAMs.Include(s => s.DVT).Include(s => s.LOAIHANG).ToList();
            }
        }
    }
}