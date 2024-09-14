using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TapHoa.Models;

namespace TapHoa.Controllers
{
    public class LOAIHANGsController : Controller
    {
        private TapHoaEntities db = new TapHoaEntities();

        // GET: LOAIHANGs
        public ActionResult Index()
        {
            return View(db.LOAIHANGs.ToList());
        }

        // GET: LOAIHANGs/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LOAIHANG lOAIHANG = db.LOAIHANGs.Find(id);
            if (lOAIHANG == null)
            {
                return HttpNotFound();
            }
            return View(lOAIHANG);
        }

        // GET: LOAIHANGs/Create
        public ActionResult Create()
        {
            return View();
        }
        private string GenerateNewMADVT()
        {
            var a = db.LOAIHANGs.OrderByDescending(d => d.MALOAI).FirstOrDefault();
            if (a == null)
            {
                return "A000"; // Nếu chưa có mã nào trong cơ sở dữ liệu
            }

            string b = a.MALOAI;
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
        // POST: LOAIHANGs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MALOAI,TENLOAI")] LOAIHANG lOAIHANG)
        {
            if (ModelState.IsValid)
            {
                lOAIHANG.MALOAI = GenerateNewMADVT();
                db.LOAIHANGs.Add(lOAIHANG);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(lOAIHANG);
        }

        // GET: LOAIHANGs/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LOAIHANG lOAIHANG = db.LOAIHANGs.Find(id);
            if (lOAIHANG == null)
            {
                return HttpNotFound();
            }
            return View(lOAIHANG);
        }

        // POST: LOAIHANGs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MALOAI,TENLOAI")] LOAIHANG lOAIHANG)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lOAIHANG).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(lOAIHANG);
        }

        // GET: LOAIHANGs/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LOAIHANG lOAIHANG = db.LOAIHANGs.Find(id);
            if (lOAIHANG == null)
            {
                return HttpNotFound();
            }
            return View(lOAIHANG);
        }

        // POST: LOAIHANGs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            try
            {
                LOAIHANG loaihang = db.LOAIHANGs.Find(id);
                db.LOAIHANGs.Remove(loaihang);
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
