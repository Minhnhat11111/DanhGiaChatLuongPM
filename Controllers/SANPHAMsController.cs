using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TapHoa.Models;

namespace TapHoa.Controllers
{
    public class SANPHAMsController : Controller
    {
        private TapHoaEntities db = new TapHoaEntities();
        
        // GET: SANPHAMs
        //public ActionResult Index()
        //{
        //    var sANPHAMs = db.SANPHAMs.Include(s => s.DVT).Include(s => s.LOAIHANG);
        //    return View(sANPHAMs.ToList());
        //}
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

        // GET: SANPHAMs/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SANPHAM sANPHAM = db.SANPHAMs.Find(id);
            if (sANPHAM == null)
            {
                return HttpNotFound();
            }
            return View(sANPHAM);
        }
        private string GenerateNewMADVT()
        {
            var a = db.SANPHAMs.OrderByDescending(d => d.MASP).FirstOrDefault();
            if (a == null)
            {
                return "A000"; // Nếu chưa có mã nào trong cơ sở dữ liệu
            }

            string b = a.MASP;
            char letterPart = b[0];
            int numericPart = int.Parse(b.Substring(1));

            numericPart++;
            if (numericPart > 999)
            {
                numericPart = 0;
                letterPart++;
                if (letterPart > 'Z')
                {
                    throw new InvalidOperationException("Đã hết mã để sử dụng.");
                }
            }

            string newMADVT = letterPart + numericPart.ToString("D3"); // Đảm bảo có 3 chữ số
            return newMADVT;
        }
        // GET: SANPHAMs/Create
        public ActionResult Create()
        {
            ViewBag.MADVT = new SelectList(db.DVTs, "MADVT", "TENDVT");
            ViewBag.MALOAI = new SelectList(db.LOAIHANGs, "MALOAI", "TENLOAI");
            return View();
        }

        // POST: SANPHAMs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MASP,TENSP,MADVT,HANGSX,GIAHIENHANH,SOLUONG,SOLUONGDABAN,HINHANH,MALOAI")] SANPHAM sANPHAM, HttpPostedFileBase HINHANH)
        {
            if (sANPHAM.SOLUONG < 0)
            {
                ModelState.AddModelError("SOLUONG", "Số lượng không được âm.");
            }
            if (ModelState.IsValid)
            {
                sANPHAM.MASP = GenerateNewMADVT();
                sANPHAM.SOLUONGDABAN = 0;
                if (HINHANH != null)
                {
                    var filename = Path.GetFileName(HINHANH.FileName);

                    
                    var path = Path.Combine(Server.MapPath("~/Images"), filename);

                    sANPHAM.HINHANH = filename;

                    HINHANH.SaveAs(path);
                }
                db.SANPHAMs.Add(sANPHAM);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MADVT = new SelectList(db.DVTs, "MADVT", "TENDVT", sANPHAM.MADVT);
            ViewBag.MALOAI = new SelectList(db.LOAIHANGs, "MALOAI", "TENLOAI", sANPHAM.MALOAI);
            return View(sANPHAM);
        }

        // GET: SANPHAMs/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SANPHAM sANPHAM = db.SANPHAMs.Find(id);
            if (sANPHAM == null)
            {
                return HttpNotFound();
            }
            ViewBag.MADVT = new SelectList(db.DVTs, "MADVT", "TENDVT", sANPHAM.MADVT);
            ViewBag.MALOAI = new SelectList(db.LOAIHANGs, "MALOAI", "TENLOAI", sANPHAM.MALOAI);
            return View(sANPHAM);
        }

        // POST: SANPHAMs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MASP,TENSP,MADVT,HANGSX,GIAHIENHANH,SOLUONG,SOLUONGDABAN,HINHANH,MALOAI")] SANPHAM sANPHAM, HttpPostedFileBase HinhAnh)
        {
            if (sANPHAM.SOLUONG < 0)
            {
                ModelState.AddModelError("SOLUONG", "Số lượng không được âm.");
            }
            if (ModelState.IsValid)
            {
                var sanpham = db.SANPHAMs.FirstOrDefault(p => p.MASP == sANPHAM.MASP);
                if(sanpham != null)
                {
                    sanpham.TENSP = sANPHAM.TENSP;
                    sanpham.GIAHIENHANH = sANPHAM.GIAHIENHANH;
                    sanpham.SOLUONG = sANPHAM.SOLUONG;
                    sanpham.SOLUONGDABAN = sANPHAM.SOLUONGDABAN;
                    sanpham.MADVT = sANPHAM.MADVT;
                    sanpham.MALOAI = sANPHAM.MALOAI;
                    if(HinhAnh != null)
                    {
                        var filename = Path.GetFileName(HinhAnh.FileName);
                        var path = Path.Combine(Server.MapPath("~/Images"), filename);
                        sanpham.HINHANH = filename;
                        HinhAnh.SaveAs(path);
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MADVT = new SelectList(db.DVTs, "MADVT", "TENDVT", sANPHAM.MADVT);
            ViewBag.MALOAI = new SelectList(db.LOAIHANGs, "MALOAI", "TENLOAI", sANPHAM.MALOAI);
            return View(sANPHAM);
        }

        // GET: SANPHAMs/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SANPHAM sANPHAM = db.SANPHAMs.Find(id);
            if (sANPHAM == null)
            {
                return HttpNotFound();
            }
            return View(sANPHAM);
        }

        // POST: SANPHAMs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                SANPHAM sanpham = db.SANPHAMs.Find(id);
                db.SANPHAMs.Remove(sanpham);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return Content("Không xóa được do có liên quan đến bảng khác");
            }
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
