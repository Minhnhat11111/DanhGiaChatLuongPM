using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TapHoa.Models;

namespace TapHoa.Controllers
{
    public class HomeController : Controller
    {
        private TapHoaEntities db = new TapHoaEntities();
        // GET: Home
        public async Task<ActionResult> Index(string searchString)
        {
            if (db.SANPHAMs == null)
            {
                return Content("Khong tim thay nhan vien co dia chi nay");
            }

            var sanpham = from e in db.SANPHAMs
                            select e;
            if (!String.IsNullOrEmpty(searchString))
            {
                sanpham = sanpham.Where(a => a.TENSP.Contains(searchString));
            }

            return View(await sanpham.ToListAsync());
        }
        // Controller Code
        public ActionResult RevenueStatistics()
        {
            return View();
        }

        public ActionResult GetRevenueData(string period, string date = null)
        {
            try
            {
                var revenueData = new RevenueDataResult();

                DateTime startDate = DateTime.Now;
                DateTime endDate = DateTime.Now;

                if (!string.IsNullOrEmpty(date))
                {
                    if (period == "day")
                    {
                        startDate = DateTime.Parse(date);
                        endDate = startDate.AddDays(1);
                        var data = db.HOADONs
                            .Where(h => h.NGHD >= startDate && h.NGHD < endDate)
                            .GroupBy(h => new { h.NGHD.Day, h.NGHD.Month, h.NGHD.Year })
                            .Select(g => new RevenueData
                            {
                                Label = g.Key.Day + "/" + g.Key.Month + "/" + g.Key.Year,
                                TotalRevenue = (decimal)g.Sum(h => h.TONGTIEN)
                            })
                            .FirstOrDefault();

                        if (data != null)
                        {
                            revenueData.Label = data.Label;
                            revenueData.TotalRevenue = data.TotalRevenue;
                        }
                    }
                    else if (period == "month")
                    {
                        startDate = DateTime.Parse(date + "-01");
                        endDate = startDate.AddMonths(1);
                        revenueData.Year = startDate.Year;
                        revenueData.Month = startDate.Month;
                        revenueData.Data = db.HOADONs
                            .Where(h => h.NGHD >= startDate && h.NGHD < endDate)
                            .GroupBy(h => h.NGHD.Day)
                            .Select(g => new RevenueData
                            {
                                Day = g.Key,
                                TotalRevenue = (decimal)g.Sum(h => h.TONGTIEN)
                            })
                            .ToList();
                    }
                    else if (period == "year")
                    {
                        startDate = new DateTime(int.Parse(date), 1, 1);
                        endDate = startDate.AddYears(1);
                        revenueData.Year = startDate.Year;
                        revenueData.Data = db.HOADONs
                            .Where(h => h.NGHD >= startDate && h.NGHD < endDate)
                            .GroupBy(h => h.NGHD.Month)
                            .Select(g => new RevenueData
                            {
                                Month = g.Key,
                                TotalRevenue = (decimal)g.Sum(h => h.TONGTIEN)
                            })
                            .ToList();
                    }
                }

                return Json(revenueData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GetRevenueData: " + ex.Message);
                return new HttpStatusCodeResult(500, "Internal server error: " + ex.Message);
            }
        }



        public class RevenueDataResult
        {
            public string Label { get; set; }
            public decimal TotalRevenue { get; set; }
            public int Year { get; set; }
            public int Month { get; set; }
            public List<RevenueData> Data { get; set; }
        }

        public class RevenueData
        {
            public string Label { get; set; }
            public decimal TotalRevenue { get; set; }
            public int Day { get; set; }
            public int Month { get; set; }
        }




        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}