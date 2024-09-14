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
    public class UserController : Controller
    {
        // GET: User
        private TapHoaEntities db = new TapHoaEntities();
        // GET: Account
        public ActionResult Index()
        {
            return View(db.NHANVIENs.ToList());
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "MANV,HOTEN,DCHI,SDT,TENDANGNHAP,MATKHAU")] NHANVIEN nhanvien)
        {
            if (db.NHANVIENs.Any(x => x.SDT == nhanvien.SDT))
            {
                ModelState.AddModelError("SDT", "Số điện thoại đã được sử dụng. Vui lòng sử dụng số khác.");
            }
            if (!IsValidUsernameOrPassword(nhanvien.TENDANGNHAP))
            {
                ModelState.AddModelError("TENDANGNHAP", "Tên đăng nhập phải có ít nhất 8 ký tự, bao gồm chữ và số.");
            }
            if (!IsValidUsernameOrPassword(nhanvien.MATKHAU))
            {
                ModelState.AddModelError("MATKHAU", "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ và số.");
            }
            if (ModelState.IsValid)
            {
                nhanvien.MANV = GenerateNewMANV();
                db.NHANVIENs.Add(nhanvien);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(nhanvien);
        }
        private bool IsValidUsernameOrPassword(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length < 8)
            {
                return false;
            }

            bool hasLetter = input.Any(char.IsLetter);
            bool hasDigit = input.Any(char.IsDigit);

            return hasLetter && hasDigit;
        }
        // GET: DVT/Edit/5
        public ActionResult Edit(String id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NHANVIEN nhanvien = db.NHANVIENs.Find(id);
            if (nhanvien == null)
            {
                return HttpNotFound();
            }
            return View(nhanvien);
        }

        // POST: DVT/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "MANV,HOTEN,DCHI,SDT,TENDANGNHAP,MATKHAU")] NHANVIEN nhanvien)
        {
            if (db.NHANVIENs.Any(x => x.SDT == nhanvien.SDT))
            {
                ModelState.AddModelError("SDT", "Số điện thoại đã được sử dụng. Vui lòng sử dụng số khác.");
            }
            if (!IsValidUsernameOrPassword(nhanvien.TENDANGNHAP))
            {
                ModelState.AddModelError("TENDANGNHAP", "Tên đăng nhập phải có ít nhất 8 ký tự, bao gồm chữ và số.");
            }
            if (!IsValidUsernameOrPassword(nhanvien.MATKHAU))
            {
                ModelState.AddModelError("MATKHAU", "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ và số.");
            }
            if (ModelState.IsValid)
            {
                db.Entry(nhanvien).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(nhanvien);
        }

        // GET: DVT/Delete/5
        public ActionResult Delete(String id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NHANVIEN nhanvien = db.NHANVIENs.Find(id);
            if (nhanvien == null)
            {
                return HttpNotFound();
            }
            return View(nhanvien);
        }

        // POST: DVT/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(String id)
        {

            try
            {
                NHANVIEN nhanvien = db.NHANVIENs.Find(id);
                db.NHANVIENs.Remove(nhanvien);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return Content("Không xóa được do có liên quan đến bảng khác");
            }
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(NHANVIEN cust)
        {
            
            if (ModelState.IsValid)
            {
                // Kiểm tra trường hợp đặc biệt không cho phép đăng nhập
                if (cust.TENDANGNHAP == "tiemtaphoa" && cust.MATKHAU == "taphoacuatui")
                {
                    ViewBag.LoginFail = "Có thể tên đăng nhập hoặc mật khẩu đã sai vui lòng kiểm tra lại !!!";
                    return View("Login");
                }
                // Kiểm tra trong bảng Admin trước
                var adminCheck = db.ADMINs.FirstOrDefault(a => a.TENDANGNHAP == cust.TENDANGNHAP && a.MATKHAU == cust.MATKHAU);
                if (adminCheck != null)
                {
                    // Nếu hợp lệ, thiết lập quyền admin và lưu trữ thông tin trong session
                    Session["ADMIN"] = adminCheck;
                    return RedirectToAction("Index", "Home"); // Điều hướng đến trang admin
                }
                // Kiểm tra trong bảng NhanVien nếu không phải admin
                var check = db.NHANVIENs.FirstOrDefault(x => x.TENDANGNHAP == cust.TENDANGNHAP && x.MATKHAU == cust.MATKHAU);
                if (check != null)
                {
                    Session["NHANVIEN"] = check;
                    return RedirectToAction("Index", "Home");
                }
                else
                    ViewBag.LoginFail = "Có thể tên đăng nhập hoặc mật khẩu đã sai vui lòng kiểm tra lại !!!";
            }
            return View("Login");
        }
        private string GenerateNewMANV()
        {
            var lastDVT = db.NHANVIENs.OrderByDescending(d => d.MANV).FirstOrDefault();
            if (lastDVT == null)
            {
                return "A000"; // Nếu chưa có mã nào trong cơ sở dữ liệu
            }

            string lastMADVT = lastDVT.MANV;
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
        public JsonResult IsPhoneNumberAvailable(string phoneNumber)
        {
            var existingUser = db.NHANVIENs.FirstOrDefault(x => x.SDT == phoneNumber);
            return Json(existingUser == null);
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(NHANVIEN cust)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db.NHANVIENs.FirstOrDefault(x => x.SDT == cust.SDT);
                if (existingUser != null)
                {
                    ViewBag.RegisterFail = "Số điện thoại đã được sử dụng.";
                    return View();
                }
                cust.MANV = GenerateNewMANV();
                db.NHANVIENs.Add(cust);
                db.SaveChanges();
            }
            return RedirectToAction("Login");
        }
        public ActionResult Logout()
        {
            Session["ADMIN"] = null;
            Session["NHANVIEN"] = null;
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public ActionResult ResetPass()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ResetPass(NHANVIEN cust)
        {
            if (cust.TENDANGNHAP == "tiemtaphoa")
            {
                return View();
            }
            var user = db.NHANVIENs.FirstOrDefault(x => x.TENDANGNHAP == cust.TENDANGNHAP);
            if (user != null)
            {
                user.MATKHAU = "a1111111"; // Reset password to default value
                db.SaveChanges();
                ViewBag.Message = "Password has been reset to the default value. Please log in with the new password.";
                return RedirectToAction("Login", "User");
            }

            ViewBag.Message = "Invalid or expired token.";
            return View();
        }

    }
}