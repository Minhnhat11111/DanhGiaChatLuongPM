using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TapHoa.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Globalization;
using System.IO;

namespace TapHoa.Controllers
{
    public class HOADONsController : Controller
    {
        private TapHoaEntities db = new TapHoaEntities();

        // GET: HOADONs
        public ActionResult Index()
        {
            var hOADONs = db.HOADONs.Include(h => h.NHANVIEN).OrderBy(h => h.SOHD);
            return View(hOADONs.ToList());
        }


        // GET: HOADONs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            HOADON hOADON = db.HOADONs
                .Include(h => h.CTHDs.Select(ct => ct.SANPHAM)) // Bao gồm cả chi tiết hóa đơn và sản phẩm
                .Include(h => h.NHANVIEN) // Bao gồm cả thông tin nhân viên tạo hóa đơn
                .FirstOrDefault(h => h.SOHD == id);

            if (hOADON == null)
            {
                return HttpNotFound();
            }

            return View(hOADON);
        }


        // GET: HOADONs/Create
        public ActionResult Create()
        {
            ViewBag.MANV = new SelectList(db.NHANVIENs, "MANV", "HOTEN");
            return View();
        }

        // POST: HOADONs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SOHD,NGHD,MANV,TRIGIA")] HOADON hOADON)
        {
            if (ModelState.IsValid)
            {
                db.HOADONs.Add(hOADON);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MANV = new SelectList(db.NHANVIENs, "MANV", "HOTEN", hOADON.MANV);
            return View(hOADON);
        }

        // GET: HOADONs/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HOADON hOADON = db.HOADONs.Find(id);
            if (hOADON == null)
            {
                return HttpNotFound();
            }
            ViewBag.MANV = new SelectList(db.NHANVIENs, "MANV", "HOTEN", hOADON.MANV);
            return View(hOADON);
        }

        // POST: HOADONs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SOHD,NGHD,MANV,TRIGIA")] HOADON hOADON)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hOADON).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MANV = new SelectList(db.NHANVIENs, "MANV", "HOTEN", hOADON.MANV);
            return View(hOADON);
        }

        // GET: HOADONs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HOADON hOADON = db.HOADONs.Find(id);
            if (hOADON == null)
            {
                return HttpNotFound();
            }
            return View(hOADON);
        }

        // POST: HOADONs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            HOADON hOADON = db.HOADONs.Find(id);
            db.HOADONs.Remove(hOADON);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ExportToExcel(DateTime? startDate, DateTime? endDate)
        {
            if (!startDate.HasValue || !endDate.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Ngày bắt đầu và ngày kết thúc là bắt buộc.");
            }

            // Thêm thời gian để đảm bảo lấy được các hóa đơn trong suốt cả ngày cuối cùng
            var endDateWithTime = endDate.Value.AddDays(1).AddTicks(-1);

            var invoices = db.HOADONs
                .Where(h => h.NGHD >= startDate && h.NGHD <= endDateWithTime)
                .Include(h => h.CTHDs.Select(ct => ct.SANPHAM))
                .ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Invoices");

                // Add header
                worksheet.Cells[1, 1].Value = "Số HD";
                worksheet.Cells[1, 2].Value = "Ngày tạo";
                worksheet.Cells[1, 3].Value = "Nhân viên tạo";
                worksheet.Cells[1, 4].Value = "Tổng số lượng";
                worksheet.Cells[1, 5].Value = "Tổng tiền";
                worksheet.Cells[1, 6].Value = "Số tiền phải trả";
                worksheet.Cells[1, 7].Value = "Số tiền thói";
                worksheet.Cells[1, 8].Value = "Mã SP";
                worksheet.Cells[1, 9].Value = "Tên SP";
                worksheet.Cells[1, 10].Value = "Số lượng";
                worksheet.Cells[1, 11].Value = "Đơn giá";

                // Add values
                var rowIndex = 2;
                decimal totalAmount = 0;
                int totalQuantity = 0;

                foreach (var invoice in invoices)
                {
                    var invoiceStartRow = rowIndex;
                    worksheet.Cells[rowIndex, 1].Value = invoice.SOHD;
                    worksheet.Cells[rowIndex, 2].Value = invoice.NGHD.ToString("dd/MM/yyyy");
                    worksheet.Cells[rowIndex, 3].Value = invoice.NHANVIEN?.HOTEN;
                    worksheet.Cells[rowIndex, 4].Value = invoice.TONGSL;
                    worksheet.Cells[rowIndex, 5].Value = invoice.TONGTIEN;
                    worksheet.Cells[rowIndex, 6].Value = invoice.PHAITRA;
                    worksheet.Cells[rowIndex, 7].Value = invoice.TIENTRALAI;

                    totalAmount += invoice.TONGTIEN ?? 0;
                    totalQuantity += invoice.TONGSL ?? 0;

                    foreach (var detail in invoice.CTHDs)
                    {
                        worksheet.Cells[rowIndex, 8].Value = detail.MASP;
                        worksheet.Cells[rowIndex, 9].Value = detail.SANPHAM?.TENSP;
                        worksheet.Cells[rowIndex, 10].Value = detail.SL;
                        worksheet.Cells[rowIndex, 11].Value = detail.DONGIA;
                        rowIndex++;
                    }

                    // Merge invoice cells for multi-row invoices
                    if (invoice.CTHDs.Count > 1)
                    {
                        worksheet.Cells[invoiceStartRow, 1, rowIndex - 1, 1].Merge = true;
                        worksheet.Cells[invoiceStartRow, 2, rowIndex - 1, 2].Merge = true;
                        worksheet.Cells[invoiceStartRow, 3, rowIndex - 1, 3].Merge = true;
                        worksheet.Cells[invoiceStartRow, 4, rowIndex - 1, 4].Merge = true;
                        worksheet.Cells[invoiceStartRow, 5, rowIndex - 1, 5].Merge = true;
                        worksheet.Cells[invoiceStartRow, 6, rowIndex - 1, 6].Merge = true;
                        worksheet.Cells[invoiceStartRow, 7, rowIndex - 1, 7].Merge = true;
                    }
                }

                // Add total row
                worksheet.Cells[rowIndex, 1].Value = "Tổng cộng";
                worksheet.Cells[rowIndex, 1, rowIndex, 3].Merge = true;
                worksheet.Cells[rowIndex, 4].Value = totalQuantity;
                worksheet.Cells[rowIndex, 5].Value = totalAmount;
                worksheet.Cells[rowIndex, 1, rowIndex, 11].Style.Font.Bold = true;

                // Format the header
                using (var range = worksheet.Cells[1, 1, 1, 11])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue);
                }

                worksheet.Cells.AutoFitColumns();

                var fileContents = package.GetAsByteArray();
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Invoices.xlsx");
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
