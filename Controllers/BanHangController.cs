using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TapHoa.Models;

namespace TapHoa.Controllers
{
    public class BanHangController : Controller
    {
        private TapHoaEntities db = new TapHoaEntities();
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

        [HttpPost]
        public ActionResult CreateOrder(OrderData orderData)
        {
            if (orderData.SanPhams == null || !orderData.SanPhams.Any())
            {
                return Json(new { success = false, message = "Không có sản phẩm nào được chọn." });
            }

            try
            {
                using (var db = new TapHoaEntities())
                {
                    // Kiểm tra người dùng hiện tại
                    var nhanVien = Session["NHANVIEN"] as NHANVIEN;
                    var admin = Session["ADMIN"] as ADMIN; // Giả sử bạn có một bảng và session cho admin

                    if (nhanVien == null && admin == null)
                    {
                        return Json(new { success = false, message = "Người dùng không hợp lệ." });
                    }

                    // Xác định MANV dựa trên việc người dùng là nhân viên hay admin
                    string manv;
                    if (admin != null)
                    {
                        manv = "A000"; // Chuyển đổi ID của admin thành chuỗi
                    }
                    else
                    {
                        manv = nhanVien.MANV;
                        System.Diagnostics.Debug.WriteLine($"Nhân viên ID: {manv}");
                    }

                    // Tạo đơn hàng mới
                    HOADON donHang = new HOADON
                    {
                        MANV = manv,
                        NGHD = DateTime.Now,
                        TONGTIEN = orderData.SanPhams.Sum(sp => sp.Gia * sp.SoLuong),
                        TONGSL = orderData.SanPhams.Sum(sp => sp.SoLuong),
                        PHAITRA = orderData.PhaiTra,
                        TIENTRALAI = orderData.TienTraLai
                    };

                    db.HOADONs.Add(donHang);
                    db.SaveChanges();

                    // Lưu thông tin các sản phẩm vào chi tiết đơn hàng
                    foreach (var sp in orderData.SanPhams)
                    {
                        var sanPhamDb = db.SANPHAMs.FirstOrDefault(p => p.MASP == sp.IdSanPham);
                        if (sanPhamDb != null)
                        {
                            sanPhamDb.SOLUONGDABAN += sp.SoLuong;
                            sanPhamDb.SOLUONG -= sp.SoLuong;

                            CTHD chitiet = new CTHD
                            {
                                SOHD = donHang.SOHD,
                                MASP = sp.IdSanPham,
                                SL = sp.SoLuong,
                                DONGIA = sp.Gia
                            };

                            db.CTHDs.Add(chitiet);
                        }
                    }

                   

                    db.SaveChanges();
                    return Json(new { success = true, message = "Đơn hàng đã được tạo thành công.", orderId = donHang.SOHD });
                }            
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Đã có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        public ActionResult PrintReceipt(int orderId)
        {
            using (var db = new TapHoaEntities())
            {
                var order = db.HOADONs.Include("CTHDs.SANPHAM").FirstOrDefault(o => o.SOHD == orderId);
                if (order == null)
                {
                    return HttpNotFound();
                }

                MemoryStream stream = new MemoryStream();

                Rectangle pageSize = new Rectangle(226, 400);
                Document document = new Document(pageSize, 10, 10, 10, 10);

                //Document document = new Document(PageSize.A4, 10, 10, 10, 10);
                PdfWriter.GetInstance(document, stream).CloseStream = false;
                document.Open();


                string fontPath = Server.MapPath("~/assets/fonts/Roboto-Regular.ttf");
                BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font font = new Font(baseFont, 12, Font.NORMAL);


                document.Add(new Paragraph("Hóa đơn bán hàng", font));
                document.Add(new Paragraph($"Mã đơn hàng: {order.SOHD}", font));
                document.Add(new Paragraph($"Ngày: {order.NGHD}", font));
                document.Add(new Paragraph($"Nhân viên: {order.NHANVIEN.HOTEN}", font));
                document.Add(new Paragraph(""));

                foreach (var item in order.CTHDs)
                {
                    document.Add(new Paragraph($"{item.SANPHAM.TENSP} - SL: {item.SL} - Giá: {item.DONGIA} VND", font));
                }

                document.Add(new Paragraph(""));
                document.Add(new Paragraph($"Tổng tiền: {order.TONGTIEN} VND", font));
                document.Add(new Paragraph($"Tiền thối: {order.TIENTRALAI} VND", font));

                document.Close();
                byte[] pdfBytes = stream.ToArray();

                return File(pdfBytes, "application/pdf", "hoa_don.pdf");
            }
        }

    }
}