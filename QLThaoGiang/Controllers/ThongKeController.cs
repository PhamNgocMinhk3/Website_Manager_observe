using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLThaoGiang.Models;
using System.IO;
using OfficeOpenXml;
namespace QLThaoGiang.Controllers
{
    public class ThongKeController : Controller
    {
        // GET: ThongKe
        dbDataContext db = new dbDataContext();
        public class NamHocModel
        {
            public int MaNH { get; set; }
            public string TenNH { get; set; }
        }
        public class HocKiModel
        {
            public int MaHK { get; set; }
            public string TenHK { get; set; }
        }
        public ActionResult Index()
        {
            var namHocList = db.NamHocs.Select(n => new NamHocModel
            {
                MaNH = n.MaNH,
                TenNH = n.TenNH
            }).ToList();

            var hocKiList = db.HocKies.Select(h => new HocKiModel
            {
                MaHK = h.MaHK,
                TenHK = h.TenHK
            }).ToList();

            ViewBag.NamHocList = namHocList;
            ViewBag.HocKiList = hocKiList;

            return View();
        }

        public class LichDataModel
        {
            public string HoTenGV { get; set; }
            public int SoLuocDangKyThaoGiang { get; set; }
            public int TongTietThaoGiang { get; set; }
            public int SoTietDuGio { get; set; }
            public int SoLuocDuGioCuaGiangVien { get; set; }
            public string TenHK { get; set; }
            public string TenNH { get; set; }
            public string MaMH { get; set; }
            public string GVC { get; set; }
            public string GVDD { get; set; }
            public string TenMonHoc { get; set; }
            public string NhomHoc { get; set; }
            public string DiaDiem { get; set; }
            public string TietBD { get; set; }
            public DateTime NgayThaoGiang { get; set; }
            public int TC { get; set; }
        }

        [HttpPost]
        public ActionResult XuLy(FormCollection f)
        {
            var tc = f["TC"];
            var namhoc = f["NamHoc"];
            var hocki = f["HocKi"];
            if (tc == "9")
            {
                string query = @"
            SELECT C.HoTenGV, ISNULL(C.TongTietThaoGiang, 0) AS TongTietThaoGiang, ISNULL(D.SoTietDuGio, 0) AS SoTietDuGio
            FROM (
            SELECT dbo.GiangVien.MaGV, dbo.GiangVien.HoTenGV, SUM(LEN(dbo.DangKyThaoGiang.TietBD) - LEN(REPLACE(dbo.DangKyThaoGiang.TietBD, '-', '')) + 1) AS TongTietThaoGiang
            FROM dbo.DangKyThaoGiang 
            INNER JOIN dbo.GiangVien ON dbo.DangKyThaoGiang.MaGV = dbo.GiangVien.MaGV";

                if (!string.IsNullOrEmpty(namhoc) || !string.IsNullOrEmpty(hocki))
                {
                    query += " WHERE ";

                    if (!string.IsNullOrEmpty(namhoc))
                    {
                        query += $" MaNH = '{namhoc}' ";
                    }

                    if (!string.IsNullOrEmpty(hocki))
                    {
                        if (!string.IsNullOrEmpty(namhoc))
                        {
                            query += " AND ";
                        }
                        query += $" MaHK = '{hocki}' ";
                    }
                }

                query += @" GROUP BY dbo.GiangVien.MaGV, dbo.GiangVien.HoTenGV
        ) AS C 
        LEFT OUTER JOIN (
            SELECT A.GVDuGio, SUM(B.SoTiet) AS SoTietDuGio
            FROM (
                SELECT dbo.DuGio.MaDK, dbo.DuGio.GVDuGio
                FROM dbo.DuGio 
                INNER JOIN dbo.PhieuCham ON dbo.PhieuCham.MaDuGio = dbo.DuGio.MaDuGio
            ) AS A 
            INNER JOIN (
                SELECT MaDK, LEN(TietBD) - LEN(REPLACE(TietBD, '-', '')) + 1 AS SoTiet
                FROM dbo.DangKyThaoGiang
            ) AS B ON A.MaDK = B.MaDK
            GROUP BY A.GVDuGio
        ) AS D ON C.MaGV = D.GVDuGio";

                ViewBag.TC = Convert.ToInt32(tc);
                var lichData = db.ExecuteQuery<LichDataModel>(query).ToList();
                return PartialView("_LichTable", lichData);
            }

            else if (tc == "4")
            {
                string query = "SELECT dbo.DangKyThaoGiang.MaMH, " +
                               "GVDangLop.HoTenGV AS GVC, " +
                               "STRING_AGG(GVDangGio.HoTenGV, ', ') AS GVDD, " +
                               "dbo.DangKyThaoGiang.TenMonHoc, " +
                               "dbo.DangKyThaoGiang.NhomHoc, " +
                               "dbo.DangKyThaoGiang.DiaDiem, " +
                               "dbo.DangKyThaoGiang.TietBD, " +
                               "dbo.NamHoc.TenNH, " +
                               "dbo.HocKy.TenHK, " +
                               "dbo.DangKyThaoGiang.NgayThaoGiang " +
                               "FROM dbo.DangKyThaoGiang " +
                               "LEFT OUTER JOIN dbo.GiangVien AS GVDangLop ON dbo.DangKyThaoGiang.MaGV = GVDangLop.MaGV " +
                               "LEFT OUTER JOIN dbo.DuGio ON dbo.DangKyThaoGiang.MaDK = dbo.DuGio.MaDK " +
                               "LEFT OUTER JOIN dbo.GiangVien AS GVDangGio ON dbo.DuGio.GVDuGio = GVDangGio.MaGV " +
                               "LEFT OUTER JOIN dbo.NamHoc ON dbo.DangKyThaoGiang.MaNH = dbo.NamHoc.MaNH " +
                               "LEFT OUTER JOIN dbo.HocKy ON dbo.DangKyThaoGiang.MaHK = dbo.HocKy.MaHK ";

                if (!string.IsNullOrEmpty(namhoc) || !string.IsNullOrEmpty(hocki))
                {
                    query += "WHERE ";

                    if (!string.IsNullOrEmpty(namhoc))
                    {
                        query += $"dbo.NamHoc.MaNH = '{namhoc}' ";
                    }

                    if (!string.IsNullOrEmpty(hocki))
                    {
                        if (!string.IsNullOrEmpty(namhoc))
                        {
                            query += "AND ";
                        }
                        query += $"dbo.HocKy.MaHK = '{hocki}' ";
                    }
                }

                query += "GROUP BY dbo.DangKyThaoGiang.MaMH, " +
                         "GVDangLop.HoTenGV, " +
                         "dbo.DangKyThaoGiang.TenMonHoc, " +
                         "dbo.DangKyThaoGiang.NhomHoc, " +
                         "dbo.DangKyThaoGiang.DiaDiem, " +
                         "dbo.DangKyThaoGiang.TietBD, " +
                         "dbo.NamHoc.TenNH, " +
                         "dbo.HocKy.TenHK, " +
                         "dbo.DangKyThaoGiang.NgayThaoGiang, " +
                         "dbo.DangKyThaoGiang.MaNH, " +
                         "dbo.DangKyThaoGiang.MaHK";
                ViewBag.TC = Convert.ToInt32(tc);
                var lichData = db.ExecuteQuery<LichDataModel>(query).ToList();
                return PartialView("_LichTable", lichData);
            }
            return Content("<p>No data to display.</p>");
        }

        private byte[] GetExcelData(List<LichDataModel> lichData, string tc)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Data");
                if (tc == "9")
                {
                    // Thêm các tiêu đề cho TC = 9
                    worksheet.Cells[1, 1].Value = "Tên Giảng Viên";
                    worksheet.Cells[1, 2].Value = "Tổng Tiết Thảo Giảng";
                    worksheet.Cells[1, 3].Value = "Số Tiết Dự Giờ";
                }
                else if (tc == "4")
                {
                    // Thêm các tiêu đề cho TC = 4
                    worksheet.Cells[1, 1].Value = "MaMH";
                    worksheet.Cells[1, 2].Value = "Giảng Viên Đứng Lớp";
                    worksheet.Cells[1, 3].Value = "Giảng Viên Dự Giờ";
                    worksheet.Cells[1, 4].Value = "Tên Môn Học";
                    worksheet.Cells[1, 5].Value = "Nhóm Học";
                    worksheet.Cells[1, 6].Value = "Địa Điểm";
                    worksheet.Cells[1, 7].Value = "Tiết Bắt Đầu";
                    worksheet.Cells[1, 8].Value = "Năm học";
                    worksheet.Cells[1, 9].Value = "Học Kì";
                    worksheet.Cells[1, 10].Value = "Ngày Thao Giảng";
                    // Thêm các tiêu đề khác nếu cần...
                }

                // Đổ dữ liệu dòng dữ liệu dựa trên TC được chọn
                for (int i = 0; i < lichData.Count; i++)
                {
                    var rowData = lichData[i];
                    if (tc == "9")
                    {
                        worksheet.Cells[i + 2, 1].Value = rowData.HoTenGV;
                        worksheet.Cells[i + 2, 2].Value = rowData.TongTietThaoGiang;
                        worksheet.Cells[i + 2, 3].Value = rowData.SoTietDuGio;
                    }
                    else if (tc == "4")
                    {
                        worksheet.Cells[i + 2, 1].Value = rowData.MaMH;
                        worksheet.Cells[i + 2, 2].Value = rowData.GVC;
                        worksheet.Cells[i + 2, 3].Value = rowData.GVDD;
                        worksheet.Cells[i + 2, 4].Value = rowData.TenMonHoc;
                        worksheet.Cells[i + 2, 5].Value = rowData.NhomHoc;
                        worksheet.Cells[i + 2, 6].Value = rowData.DiaDiem;
                        worksheet.Cells[i + 2, 7].Value = rowData.TietBD;
                        worksheet.Cells[i + 2, 8].Value = rowData.TenNH;
                        worksheet.Cells[i + 2, 9].Value = rowData.TenHK;
                        string formattedDate = rowData.NgayThaoGiang.ToString("yyyy-MM-dd");
                        worksheet.Cells[i + 2, 10].Value = formattedDate;
                    }
                }

                // Tự động điều chỉnh cột để dễ đọc hơn
                worksheet.Cells.AutoFitColumns();

                // Trả về dữ liệu tệp Excel dưới dạng mảng byte
                return package.GetAsByteArray();
            }
        }
        [HttpPost]
        public ActionResult ExportToExcel(FormCollection form)
        {
            // Xử lý logic để lấy dữ liệu và TC từ form
            var tc = form["TC"];
            var namhoc = Convert.ToInt32(form["NamHoc"]);
            var hocki = Convert.ToInt32(form["HocKi"]);

            // Xử lý logic để lấy dữ liệu tương ứng với TC
            List<LichDataModel> lichData = new List<LichDataModel>();
            if (tc == "9")
            {
                string query = @"
            SELECT C.HoTenGV, ISNULL(C.TongTietThaoGiang, 0) AS TongTietThaoGiang, ISNULL(D.SoTietDuGio, 0) AS SoTietDuGio
            FROM (
            SELECT dbo.GiangVien.MaGV, dbo.GiangVien.HoTenGV, SUM(LEN(dbo.DangKyThaoGiang.TietBD) - LEN(REPLACE(dbo.DangKyThaoGiang.TietBD, '-', '')) + 1) AS TongTietThaoGiang
            FROM dbo.DangKyThaoGiang 
            INNER JOIN dbo.GiangVien ON dbo.DangKyThaoGiang.MaGV = dbo.GiangVien.MaGV";

                if (namhoc > 0 || hocki > 0)
                {
                    query += " WHERE ";

                    if (namhoc > 0)
                    {
                        query += $" MaNH = '{namhoc}' ";
                    }

                    if (hocki > 0)
                    {
                        if (namhoc > 0)
                        {
                            query += " AND ";
                        }
                        query += $" MaHK = '{hocki}' ";
                    }
                }

                query += @" GROUP BY dbo.GiangVien.MaGV, dbo.GiangVien.HoTenGV
                ) AS C 
                LEFT OUTER JOIN (
                    SELECT A.GVDuGio, SUM(B.SoTiet) AS SoTietDuGio
                    FROM (
                        SELECT dbo.DuGio.MaDK, dbo.DuGio.GVDuGio
                        FROM dbo.DuGio 
                        INNER JOIN dbo.PhieuCham ON dbo.PhieuCham.MaDuGio = dbo.DuGio.MaDuGio
                    ) AS A 
                    INNER JOIN (
                        SELECT MaDK, LEN(TietBD) - LEN(REPLACE(TietBD, '-', '')) + 1 AS SoTiet
                        FROM dbo.DangKyThaoGiang
                    ) AS B ON A.MaDK = B.MaDK
                    GROUP BY A.GVDuGio
                ) AS D ON C.MaGV = D.GVDuGio";

                var lichDataForExport = db.ExecuteQuery<LichDataModel>(query).ToList(); // Rename lichData to avoid conflict
                byte[] excelDataForExport = GetExcelData(lichDataForExport, tc); // Rename excelData to avoid conflict
                return File(excelDataForExport, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Data.xlsx");
            }
            else if (tc == "4")
            {
                // Xử lý logic để lấy dữ liệu khi TC = 4
                string query = "SELECT dbo.DangKyThaoGiang.MaMH, " +
                               "GVDangLop.HoTenGV AS GVC, " +
                               "STRING_AGG(GVDangGio.HoTenGV, ', ') AS GVDD, " +
                               "dbo.DangKyThaoGiang.TenMonHoc, " +
                               "dbo.DangKyThaoGiang.NhomHoc, " +
                               "dbo.DangKyThaoGiang.DiaDiem, " +
                               "dbo.DangKyThaoGiang.TietBD, " +
                               "dbo.NamHoc.TenNH, " +
                               "dbo.HocKy.TenHK, " +
                               "dbo.DangKyThaoGiang.NgayThaoGiang " +
                               "FROM dbo.DangKyThaoGiang " +
                               "LEFT OUTER JOIN dbo.GiangVien AS GVDangLop ON dbo.DangKyThaoGiang.MaGV = GVDangLop.MaGV " +
                               "LEFT OUTER JOIN dbo.DuGio ON dbo.DangKyThaoGiang.MaDK = dbo.DuGio.MaDK " +
                               "LEFT OUTER JOIN dbo.GiangVien AS GVDangGio ON dbo.DuGio.GVDuGio = GVDangGio.MaGV " +
                               "LEFT OUTER JOIN dbo.NamHoc ON dbo.DangKyThaoGiang.MaNH = dbo.NamHoc.MaNH " +
                               "LEFT OUTER JOIN dbo.HocKy ON dbo.DangKyThaoGiang.MaHK = dbo.HocKy.MaHK ";

                if (namhoc > 0 || hocki > 0)
                {
                    query += "WHERE ";

                    if (namhoc > 0)
                    {
                        query += $"dbo.NamHoc.MaNH = '{namhoc}' ";
                    }

                    if (hocki > 0)
                    {
                        if (namhoc > 0)
                        {
                            query += "AND ";
                        }
                        query += $"dbo.HocKy.MaHK = '{hocki}' ";
                    }
                }

                query += "GROUP BY dbo.DangKyThaoGiang.MaMH, " +
                         "GVDangLop.HoTenGV, " +
                         "dbo.DangKyThaoGiang.TenMonHoc, " +
                         "dbo.DangKyThaoGiang.NhomHoc, " +
                         "dbo.DangKyThaoGiang.DiaDiem, " +
                         "dbo.DangKyThaoGiang.TietBD, " +
                         "dbo.NamHoc.TenNH, " +
                         "dbo.HocKy.TenHK, " +
                         "dbo.DangKyThaoGiang.NgayThaoGiang, " +
                         "dbo.DangKyThaoGiang.MaNH, " +
                         "dbo.DangKyThaoGiang.MaHK";

                lichData = db.ExecuteQuery<LichDataModel>(query).ToList();
            }

            // Tạo dữ liệu Excel
            byte[] excelData = GetExcelData(lichData, tc);

            // Trả về dữ liệu Excel như một tệp Excel trực tiếp cho người dùng
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Data.xlsx");
        }

    }
}
