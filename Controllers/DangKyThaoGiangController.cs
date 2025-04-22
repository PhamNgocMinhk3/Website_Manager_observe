using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using QLThaoGiang.Models;

namespace QLThaoGiang.Controllers
{
    public class DangKyThaoGiangController : Controller
    {
        // GET: DangKyThaoGiang
        private dbDataContext db = new dbDataContext();



        [HttpGet]
        public ActionResult DK()
        {
            if (Session["Ma"] == null)
            {
                return RedirectToAction("DangNhap", "User");
            }
            int currentYear = DateTime.Now.Year;

            // Lấy 2 mục từ danh sách năm học, với điều kiện TenNH chứa năm hiện tại
            var namHocs = db.NamHocs
                .Where(n => n.TenNH.Contains(currentYear.ToString()))
                .OrderByDescending(n => n.MaNH)
                .Take(2)
                .ToList();

            var hocKys = db.HocKies.ToList();

            ViewBag.NamHocList = new SelectList(namHocs, "MaNH", "TenNH");
            ViewBag.HocKyList = new SelectList(hocKys, "MaHK", "TenHK");




            return View();
        }
        [HttpPost]
        public ActionResult DK(FormCollection f)
        {
            DangKyThaoGiang a = new DangKyThaoGiang();
            ViewBag.MaHK = new SelectList(db.HocKies.ToList().OrderBy(n => n.TenHK), "MaHK", "TenHK");
            a.MaGV = (int)Session["Ma"];
            a.TietBD = f["TietBD"];
            a.NgayThaoGiang = Convert.ToDateTime(f["NgayThaoGiang"]);
            a.MaMH = f["MaMH"];
            a.DiaDiem = f["DiaDiem"];
            a.TenBaiHoc = f["TenBaiHoc"];
            a.NhomHoc = f["Nhom"];
            a.GhiChu = f["GhiChu"];
            a.TenMonHoc = f["TenMon"];
            a.MaHK = int.Parse(f["MaHK"]);
            a.MaNH = int.Parse(f["MaNH"]);
            db.DangKyThaoGiangs.InsertOnSubmit(a);
            db.SubmitChanges();
            return View("ThanhCong");
        }
        public ActionResult XemLichGiang()
        {

            if (Session["TaiKhoan"] != null)
            {
                int id = (int)Session["Ma"];
                var ds = (from s in db.DangKyThaoGiangs where s.MaGV == id select s).ToList();
                ViewBag.kt = ds.Count();
                return View(ds);
            }
            else
            {
                return RedirectToAction("DangNhap", "User");
            }
            
        }

        public ActionResult DangSachThaoGiang(int? page)
        {
            try
            {
                GiangVien b = (GiangVien)Session["Taikhoan"];
                int iSize = 3;
                int pagenumber = (page ?? 1);
                // Lấy danh sách các buổi thảo giảng chưa đăng ký dự giờ
                var danhSachThaoGiang = db.DangKyThaoGiangs.Where(n => !db.DuGios.Any(d => d.MaDK == n.MaDK && d.GVDuGio == b.MaGV)).ToList();

                return View(danhSachThaoGiang.ToPagedList(pagenumber, iSize));
            }
            catch
            {
                return RedirectToAction("DangNhap","User");
            }
            


        }
        [HttpPost]
        public ActionResult Search(string searchField, string keyword)
        {
            IQueryable<DangKyThaoGiang> foods = db.DangKyThaoGiangs;
            if (!string.IsNullOrEmpty(keyword))
            {
                switch (searchField)
                {
                    case "1":
                        foods = foods.Where(f => f.TietBD.ToString().Contains(keyword));
                        break;
                    case "2":
                        foods = foods.Where(f => f.MaMH.ToString().Contains(keyword));
                        break;
                    case "3":
                        foods = foods.Where(f => f.HocKy.TenHK.ToString().Contains(keyword));
                        break;
                    case "4":
                        foods = foods.Where(f => f.GiangVien.HoTenGV.ToString().Contains(keyword));
                        break;
                }
            }


            return PartialView("_listThaoGiang", foods);
        }

        public ActionResult ChiTietThaoGiang(int id)
        {


            if (Session["Ma"] == null)
            {
                return RedirectToAction("DangNhap", "User");
            }

            var tg = db.DangKyThaoGiangs.SingleOrDefault(n => n.MaDK == id);
            ViewBag.MaDK = id;
            return View(tg);
        }

        public JsonResult Xoa(int id)
        {
            try
            {
                var tb = db.ThongBaos.Where(n => n.MaDK == id);
                db.ThongBaos.DeleteAllOnSubmit(tb);
                db.SubmitChanges();
                var thaoGiang = db.DangKyThaoGiangs.SingleOrDefault(n => n.MaDK == id);
                db.DangKyThaoGiangs.DeleteOnSubmit(thaoGiang);
                
                db.SubmitChanges();


                return Json(new { code = 200 }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { code = 500 }, JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult CTThaoGiang(int id)
        {
            
            var thaoGiang = db.DangKyThaoGiangs.SingleOrDefault(n => n.MaDK == id);
            ViewBag.madk = id;
            int currentYear = DateTime.Now.Year;
            var namHocs = db.NamHocs
                .Where(n => n.TenNH.Contains(currentYear.ToString()))
                .OrderByDescending(n => n.MaNH)
                .Take(2)
                .ToList();
            ViewBag.NamHocList = namHocs.Any() ? new SelectList(namHocs, "MaNH", "TenNH") : null;
            return View(thaoGiang);
        }
        public ActionResult CapNhatThaoGiang(FormCollection f)
        {
            if (Session["Ma"] == null)
            {
                return RedirectToAction("DangNhap", "User");
            }
            int ma = int.Parse(f["ma"]);
            DangKyThaoGiang a= db.DangKyThaoGiangs.SingleOrDefault(n => n.MaDK == ma);
            a.TietBD = f["TietBD"];
            a.NgayThaoGiang = Convert.ToDateTime(f["NgayThaoGiang"]);
            a.MaMH = f["MaMH"];
            a.DiaDiem = f["DiaDiem"];
            a.TenBaiHoc = f["TenBaiHoc"];
            a.NhomHoc = f["Nhom"];
            a.GhiChu = f["GhiChu"];
            a.TenMonHoc = f["TenMon"];
            a.MaHK = int.Parse(f["MaHK"]);
            a.MaNH = int.Parse(f["MaNH"]);
            ThongBao b = new ThongBao();
            b.MaDK = ma;
            db.ThongBaos.InsertOnSubmit(b);
            db.SubmitChanges();
            string query = "select * from DuGio Where MaDK= " + ma;
            var dsCTThongbaonew = db.ExecuteQuery<DuGio>(query).ToList();
            foreach (var item in dsCTThongbaonew)
            {
                CT_ThongBao ctThongBao = new CT_ThongBao();
                ctThongBao.MaTB = b.MaTB; 
                ctThongBao.NguoiNhan = item.GVDuGio;
                ctThongBao.TrangThaiDoc = false;
                db.CT_ThongBaos.InsertOnSubmit(ctThongBao);
            }

            db.SubmitChanges();
            return RedirectToAction("CTThaoGiang", new { id = ma });
            
        }
        public ActionResult TinNhan()
        {
            int ma = (int)Session["Ma"];
            var tb = db.CT_ThongBaos.Where(n => n.NguoiNhan == ma);
            ViewBag.sl = tb.Count();
            return PartialView(tb);
        }

        public ActionResult XemCTThaoGiang(int matb)
        {
            int nguoiNh = (int)Session["Ma"];
            var cttb = db.CT_ThongBaos.SingleOrDefault(n => n.MaTB == matb && n.NguoiNhan == nguoiNh);
            cttb.TrangThaiDoc = true;
            db.SubmitChanges();
            var ma = db.ThongBaos.SingleOrDefault(n => n.MaTB == matb);
            var tg = db.DangKyThaoGiangs.SingleOrDefault(n => n.MaDK== ma.MaDK);
           
            return View(tg);
        }
        public ActionResult SoLTin()
        {
            int ma = (int)Session["Ma"];
            var sl = db.CT_ThongBaos.Where(n => n.TrangThaiDoc == false && n.NguoiNhan == ma);
            ViewBag.sl = sl.Count();
            return PartialView();
        }
        public ActionResult ThanhCong()
        {

            return View();
        }

    }
}