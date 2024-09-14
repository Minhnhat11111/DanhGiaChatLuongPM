using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TapHoa.Models;

namespace TapHoa.Controllers
{
    public class DVTController : Controller
    {
        private TapHoaEntities db = new TapHoaEntities();
        // GET: DVT
        public ActionResult Index()
        {
            return View(db.DVTs.ToList());
        }

        // GET: DVT/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DVT/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DVT/Create
        private string GenerateNewMADVT()
        {
            var lastDVT = db.DVTs.OrderByDescending(d => d.MADVT).FirstOrDefault();
            if (lastDVT == null)
            {
                return "A000"; // Nếu chưa có mã nào trong cơ sở dữ liệu
            }

            string lastMADVT = lastDVT.MADVT;
            char letterPart = lastMADVT[0];
            int numericPart = int.Parse(lastMADVT.Substring(1));

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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MADVT,TENDVT")] DVT donvitinh)
        {
            if (ModelState.IsValid)
            {
                donvitinh.MADVT = GenerateNewMADVT();
                db.DVTs.Add(donvitinh);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(donvitinh);
        }

        // GET: DVT/Edit/5
        public ActionResult Edit(String id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DVT donvitinh = db.DVTs.Find(id);
            if (donvitinh == null)
            {
                return HttpNotFound();
            }
            return View(donvitinh);
        }

        // POST: DVT/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MADVT,TENDVT")] DVT donvitinh)
        {
            if(ModelState.IsValid)
            {
                db.Entry(donvitinh).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(donvitinh);
        }

        // GET: DVT/Delete/5
        public ActionResult Delete(String id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DVT donvitinh = db.DVTs.Find(id);
            if (donvitinh == null)
            {
                return HttpNotFound();
            }
            return View(donvitinh);
        }

        // POST: DVT/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(String id)
        {

            try
            {
                DVT donvitinh = db.DVTs.Find(id);
                db.DVTs.Remove(donvitinh);
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
