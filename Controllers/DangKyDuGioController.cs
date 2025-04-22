using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLThaoGiang.Models;
using System.Data.Linq;
using PagedList;
using System.Data.SqlClient;


namespace QLThaoGiang.Controllers
{
    public class DangKyDuGioController : Controller
    {
        // GET: DangKyDuGio
        private dbDataContext db = new dbDataContext();

        public ActionResult Index()
        {

            return View();
        }
        public ActionResult DKDG(int id)
        {
            GiangVien b = (GiangVien)Session["Taikhoan"];
            DuGio a = new DuGio();
            a.GVDuGio = b.MaGV;
            a.MaDK = id;

            a.ThuKy = false;
            //kiểm tra có trùng lịch thao giảng không
            var thaogiang = db.DangKyThaoGiangs.SingleOrDefault(n => n.MaDK == id);
            var kt = db.DangKyThaoGiangs.SingleOrDefault(n => n.MaGV == b.MaGV && n.TietBD == thaogiang.TietBD && n.NgayThaoGiang == thaogiang.NgayThaoGiang);

            if (kt != null)
            {
                ViewBag.Loi = "ko cho phep";
                return RedirectToAction("ChiTietThaoGiang/" + id.ToString(), "DangKyThaoGiang");
            }
            //ket thu kiem tra
            db.DuGios.InsertOnSubmit(a);
            db.SubmitChanges();
            return View("ThanhCong");
        }
        public ActionResult ThanhCong()
        {

            return View();
        }
        public ActionResult XemDKDG()
        {
            if (Session["TaiKhoan"] != null)
            {
                int id = (int)Session["Ma"];
                var ds = (from s in db.DuGios where s.GVDuGio == id select s).Where(n => !db.PhieuChams.Any(d => d.MaDuGio == n.MaDuGio)).ToList();
                // hiển thị các cuộc họp chưa lập phiếu
                ViewBag.SL = ds.Count();
                return View(ds);
            }
            else
            {
                return RedirectToAction("DangNhap", "User");
            }

        }
        public ActionResult TieuChi()
        {
            var tc = db.TieuChis.ToList();
            return PartialView(tc);
        }
        public ActionResult NhanXet()
        {
            var nx = db.NhanXets.ToList();
            return PartialView(nx);
        }
        public ActionResult PhieuCham(int id)
        {
            var kq = db.DuGios.SingleOrDefault(n => n.MaDuGio == id);
            ViewBag.madg = id;
            return View(kq);
        }
        [ValidateInput(false)]
        public ActionResult ThemPhieuCham(FormCollection f)
        {
            try
            {
                PhieuCham a = new PhieuCham();
                a.MaDuGio = int.Parse(f["MaDuGio"]);
                a.NgayDanhGia = DateTime.Now;
                a.GVDanhGia = Session["Ten"].ToString();
                db.PhieuChams.InsertOnSubmit(a);
                db.SubmitChanges();
                var sltc = db.TieuChis.ToList();

                var MaPhieu = db.PhieuChams.SingleOrDefault(n => n.MaDuGio == a.MaDuGio && n.GVDanhGia == a.GVDanhGia);
                for (int i = 1; i <= sltc.Count; i++)
                {
                    CTDanhGia b = new CTDanhGia();
                    b.MaTC = i;
                    b.MaPhieuCham = MaPhieu.MaPhieuCham;
                    b.Diem = int.Parse(f[$"Diem{i}"]);
                    db.CTDanhGias.InsertOnSubmit(b);
                    db.SubmitChanges();
                }
                var slnx = db.NhanXets.ToList();
                for (int i = 1; i <= slnx.Count; i++)
                {
                    CTNhanXet c = new CTNhanXet();
                    c.MaNX = i;
                    c.MaPhieuCham = MaPhieu.MaPhieuCham;
                    c.NoiDung = f[$"NX{i}"].ToString();
                    db.CTNhanXets.InsertOnSubmit(c);
                    db.SubmitChanges();
                }
                return RedirectToAction("XemLaiCacPhieuCham");

            }
            catch
            {
                return RedirectToAction("DangNhap", "User");
            }
            
        }
        public ActionResult XemBienBan(int? page)
        {
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            int id = (int)Session["Ma"];
            string truyvan = "select MaDuGio,  NgayThaoGiang,TietBD, B.HoTenGV, DiaDiem  from ( " +
               "SELECT dbo.DuGio.MaDuGio, dbo.DangKyThaoGiang.NgayThaoGiang, dbo.DangKyThaoGiang.TietBD, dbo.DangKyThaoGiang.DiaDiem, dbo.DangKyThaoGiang.MaGV "+
               "FROM     dbo.DuGio INNER JOIN dbo.DangKyThaoGiang ON dbo.DuGio.MaDK = dbo.DangKyThaoGiang.MaDK "+
             "WHERE(dbo.DuGio.ThuKy = 1) AND(dbo.DuGio.GVDuGio = " + id  + ")) T join GiangVien B on T.MaGV = B.MaGV";
            var ds = db.ExecuteQuery<DSBienBanGV>(truyvan).
                Select(item => new DSBienBanGV
                    {
                        MaDuGio = item.MaDuGio,
                        NgayThaoGiang = item.NgayThaoGiang,
                        TietBD = item.TietBD,
                        HoTenGV = item.HoTenGV,
                        DiaDiem = item.DiaDiem
                    })
                      .ToList()
                      .ToPagedList(pageNumber, pageSize); ;
            ViewBag.kt = ds.Count();
            return View(ds);
        }
        public ActionResult XuLyAnhBienBan(int maDG)
        {
            int diem = db.BienBans.Where(n => n.MaDuGio ==maDG).Count();
            if(diem == 0)
            {
                return PartialView("ChuaXacNhan");
            }
            else
            {
                return PartialView("DaXacNhan");
            }
           
        }
        
        public ActionResult XuLyNutBienBan(int maDG)
        {

            
            int diem = db.BienBans.Where(n => n.MaDuGio == maDG).Count();
         
            ViewBag.MaDK= db.DuGios
                  .Where(d => d.MaDuGio == maDG)
                  .Select(d => d.MaDK)
                  .FirstOrDefault();

            if (diem == 0)
            {
                return PartialView("ChuaXacNhan_B");
            }
            else
            {
                
                return PartialView("DaXacNhan_B");
            }

        }
        
        public ActionResult CapQuyenThuKy(FormCollection f)
        {
            try
            {
                int MaDuGio = int.Parse(f["ChonThuKy"]);
                var dg = db.DuGios.SingleOrDefault(n => n.MaDuGio == MaDuGio);
                dg.ThuKy = true;
                db.SubmitChanges();

                return RedirectToAction("XemDSDuGio");
            }
            catch
            {
                return RedirectToAction("XemDSDuGio");
            }
            
        }
        [HttpGet]
        public JsonResult DuLieu(int ma)
        {
            try
            {
                var NL = (from s in db.DuGios
                          where (s.MaDK == ma)
                          select new
                          {
                              Ten = s.GiangVien.HoTenGV,
                              MaDG = s.MaDuGio,

                          }).ToList();

                return Json(new { code = 200, nglieu = NL }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { code = 500 }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult XemDSDuGio()
        {
            
            var query = @"select MaDK, DiaDiem, NgayThaoGiang, TietBD, 
		(select top 1 HoTenGV from GiangVien G where G.MaGV = A.MaGV) as GiaoVien, ThuKy, 
		(select top 1 HoTenGV from GiangVien G where G.MaGV = A.GVDuGio) as GVDuGio,MaHK from
         (select T.MaDK, DiaDiem, NgayThaoGiang, TietBD, MaGV, ThuKy, GVDuGio, MaHK from
         (SELECT  MaDK, MAX(CAST(ThuKy AS INT)) AS ThuKy, 
              ISNULL(
              (SELECT TOP 1 GVDuGio FROM DuGio WHERE MaDK = d.MaDK AND ThuKy = 1),
              (SELECT TOP 1 GVDuGio FROM DuGio WHERE MaDK = d.MaDK)
                    ) AS GVDuGio
                FROM DuGio d GROUP BY MaDK ) T join DangKyThaoGiang D on T.MaDK = D.MaDK) A";

            var danhSachThaoGiang = db.ExecuteQuery<DS>(query).ToList();

            return View(danhSachThaoGiang);

        }
        [HttpPost]
        public JsonResult XemDSDuGio(FormCollection f)
        {
            try
            {
                string tuKhoa = f["tukhoa"].Trim();
                string tieuChi = f["tc"];
                string hocKy = f["hk"];
                string namHoc = f["nh"];
                int chuongtrinh = int.Parse(f["ct"]);

                var query = @"select MaDK, DiaDiem, NgayThaoGiang, TietBD, GiaoVien, ThuKy, GVDuGio,MaHK, MaCT from (
		                select MaDK, DiaDiem, NgayThaoGiang, TietBD, 
		                (select top 1 HoTenGV from GiangVien G where G.MaGV = A.MaGV) as GiaoVien, ThuKy, 
		                (select top 1 HoTenGV from GiangVien G where G.MaGV = A.GVDuGio) as GVDuGio,MaHK, MaGV from
                         (select T.MaDK, DiaDiem, NgayThaoGiang, TietBD, MaGV, ThuKy, GVDuGio, MaHK from
                         (SELECT  MaDK, MAX(CAST(ThuKy AS INT)) AS ThuKy, 
                              ISNULL(
                              (SELECT TOP 1 GVDuGio FROM DuGio WHERE MaDK = d.MaDK AND ThuKy = 1),
                              (SELECT TOP 1 GVDuGio FROM DuGio WHERE MaDK = d.MaDK)
                                    ) AS GVDuGio
                                FROM DuGio d GROUP BY MaDK ) T join DangKyThaoGiang D on T.MaDK = D.MaDK) A )B join GiangVien G on B.MaGV=G.MaGV";

                var danhSachThaoGiang = db.ExecuteQuery<DS>(query);
                if (tieuChi == "2")
                {
                    danhSachThaoGiang = danhSachThaoGiang.Where(n => n.GiaoVien.Contains(tuKhoa));
                }
                if (tieuChi == "3")
                {
                    danhSachThaoGiang = danhSachThaoGiang.Where(n => n.TietBD.Contains(tuKhoa));
                }

                if (tieuChi == "4")
                {
                    danhSachThaoGiang = danhSachThaoGiang.Where(n => n.DiaDiem.Contains(tuKhoa));
                }
                if (hocKy == "1")
                {
                    danhSachThaoGiang = danhSachThaoGiang.Where(n => n.MaHK == 1);
                }
                if (hocKy == "2")
                {
                    danhSachThaoGiang = danhSachThaoGiang.Where(n => n.MaHK == 2);
                }
                // năm học thì truy vấn thẳng
                danhSachThaoGiang = danhSachThaoGiang.Where(n => n.NgayThaoGiang.Year >= int.Parse(namHoc));
                if (chuongtrinh > 0)
                {
                    danhSachThaoGiang = danhSachThaoGiang.Where(n => n.MaCT == chuongtrinh);
                }
                ViewBag.tukhoa = tuKhoa;
                return Json(new { code = 200, danhsach = danhSachThaoGiang }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { code = 500 }, JsonRequestBehavior.AllowGet);

            }        
            
        }


        public ActionResult XemLaiCacPhieuCham()
        {
            var ds = db.PhieuChams.Where(n => n.DuGio.GVDuGio == (int)Session["Ma"]);
            ViewBag.kt = ds.Count();
            return View(ds);
        }

        public ActionResult XemPhieu(int ma)
        {

            ViewBag.Ma = ma;

            return View();
        }

        public ActionResult DauTrangXemLaiPhieu(int ma)
        {
            var tt = db.PhieuChams.SingleOrDefault(n => n.MaPhieuCham == ma);
            return PartialView("_DauTrangXemLaiPhieu", tt);
        }
        public ActionResult NhanXetXemLai(int ma)
        {
            var nxs = db.CTNhanXets.Where(n => n.MaPhieuCham == ma);
            return PartialView("_NhanXetXemLai", nxs);
        }
        public ActionResult DiemPhieuChamXemLai(int ma)
        {
            var diems = db.CTDanhGias.Where(n => n.MaPhieuCham == ma);
            ViewBag.Tong = diems.Sum(n => n.Diem);
            return PartialView("_DiemPhieuChamXemLai", diems);
        }
        public ActionResult XemTatCaBienBan(int? page)
        {
            
            if (Session["TaiKhoan"] != null)
            {
                int pageSize = 8;
                int pageNumber = (page ?? 1);
                var truy = @"select P.* from( select MaDuGio, NgayThaoGiang, TietBD, B.HoTenGV, DiaDiem from(
                           SELECT dbo.DuGio.MaDuGio, dbo.DangKyThaoGiang.NgayThaoGiang, dbo.DangKyThaoGiang.TietBD, dbo.DangKyThaoGiang.DiaDiem, dbo.DangKyThaoGiang.MaGV 
                           FROM dbo.DuGio INNER JOIN dbo.DangKyThaoGiang ON dbo.DuGio.MaDK = dbo.DangKyThaoGiang.MaDK 
                           WHERE(dbo.DuGio.ThuKy = 1) ) T join GiangVien B on T.MaGV = B.MaGV) P join BienBan C on P.MaDuGio = C.MaDuGio";
                var ds = db.ExecuteQuery<DSBienBanGV>(truy).
                        Select(item => new DSBienBanGV
                        {
                            MaDuGio = item.MaDuGio,
                            NgayThaoGiang = item.NgayThaoGiang,
                            TietBD = item.TietBD,
                            HoTenGV = item.HoTenGV,
                            DiaDiem = item.DiaDiem
                        })
                          .ToList()
                          .ToPagedList(pageNumber, pageSize);
                ViewBag.kt = ds.Count();
                return View("XemBienBan", ds);
                
            }
            else
            {
                return RedirectToAction("DangNhap", "User");
            }

           
        }
        public ActionResult LapBienBan(int MaDK)
        {
            ViewBag.MaDK = MaDK;
            var diem = (from dg in db.DuGios
                        join bb in db.BienBans on dg.MaDuGio equals bb.MaDuGio
                        where dg.MaDK == MaDK && dg.ThuKy == true
                        select dg.MaDuGio).Count();
            if (diem == 0)
            {
                ViewBag.kt = 0;// chua lap bien ban
            }
            else
            {  
                ViewBag.kt = 1;// đã lập rồi
               ViewBag.gd = db.GiangViens.SingleOrDefault(n => n.MaCV==1).HoTenGV.ToString();
                var truy = @"SELECT ISNULL(M.Tong, 0) AS Tong
                                 FROM DuGio D
                                 LEFT JOIN (
                                     SELECT dbo.PhieuCham.MaDuGio, SUM(dbo.CTDanhGia.Diem) AS Tong
                                     FROM dbo.PhieuCham
                                     INNER JOIN dbo.CTDanhGia ON dbo.PhieuCham.MaPhieuCham = dbo.CTDanhGia.MaPhieuCham
                                     GROUP BY dbo.PhieuCham.MaDuGio
                                 ) M ON D.MaDuGio = M.MaDuGio
                                 WHERE MaDK = " + MaDK;

                var ds = db.ExecuteQuery<Loai>(truy).ToList();
                int sl = ds.Count();
                Double tongdiem= (from p in ds select p.Tong).Sum();
                ViewBag.dtb = tongdiem / sl;
            }
                        
            return View();
        }
        
        public ActionResult XemDSThanhVien(int ma)
        {
              int gv = (int)Session["Ma"];
            var truyVan = "select A.MaDuGio, A.HoTenGV, ISNULL(B.Tong, 0) as Tong from(SELECT dbo.DuGio.MaDuGio, dbo.GiangVien.HoTenGV" +
                          " FROM     dbo.DuGio INNER JOIN dbo.GiangVien ON dbo.DuGio.GVDuGio = dbo.GiangVien.MaGV" +
                          " WHERE  (dbo.DuGio.MaDK = " + ma + ")) A left join(SELECT dbo.PhieuCham.MaDuGio, SUM(dbo.CTDanhGia.Diem) AS Tong" +
                          " FROM dbo.PhieuCham INNER JOIN dbo.CTDanhGia ON dbo.PhieuCham.MaPhieuCham = dbo.CTDanhGia.MaPhieuCham" +
                          " GROUP BY dbo.PhieuCham.MaDuGio) B on A.MaDuGio = B.MaDuGio";
                var ds = db.ExecuteQuery<DSDiem>(truyVan).ToList();
                ViewBag.madg= db.DuGios
                .Where(dg => dg.GVDuGio == gv && dg.MaDK == ma)
                .Select(dg => dg.MaDuGio)
                .FirstOrDefault();
            Double diemtatca = (from bb in ds select bb.Tong).Sum();
            ViewBag.sl = ds.Count();
            ViewBag.tb= diemtatca / ViewBag.sl;
            
            return PartialView(ds);
                     
            
        }
        public ActionResult ThongTinBienBan(int maDK)
        {
            
            var thongtin = db.DangKyThaoGiangs.SingleOrDefault(n => n.MaDK== maDK);
            ViewBag.madg = db.DuGios
                .Where(dg => dg.MaDK == maDK && dg.ThuKy==true )
                .Select(dg => dg.MaDuGio)
                .FirstOrDefault();
            int mem = ViewBag.madg;
            ViewBag.maphieu= db.PhieuChams.Where(p => p.MaDuGio==mem)
                .Select(d => d.MaPhieuCham)
                .FirstOrDefault();
            return PartialView(thongtin);


        }
        public ActionResult TongHopNhanXet(int maDK, int MaNX)
        {// truy van theo ma NX de bieets can nx nao vaf cuc hop nao
            var truy = "select NoiDung from (select MaPhieuCham from"+
                            "(select * from PhieuCham) T join(select * from DuGio Where MaDK = "+ maDK +" ) P"+
                             " on T.MaDuGio = P.MaDuGio) A join(select* from CTNhanXet where MaNX= "+ MaNX +") B"+
                             " on A.MaPhieuCham = B.MaPhieuCham";
            var ds = db.ExecuteQuery<NX>(truy).ToList();
            return PartialView(ds);
        }

        public ActionResult XacNhanBienBan(FormCollection f)
        {
            BienBan bb = new BienBan();
            bb.MaGV = (int)Session["Ma"];
            bb.MaDuGio = int.Parse(f["madg"]);
            bb.TongDiem= int.Parse(f["TongDiem"]);
            bb.SiSo = int.Parse(f["SiSo"]);
            db.BienBans.InsertOnSubmit(bb);
            db.SubmitChanges();
            return RedirectToAction("XemBienBan");
        }

        public ActionResult NhapSiSo(int maDG)
        {
            int diem = db.BienBans.Where(n => n.MaDuGio == maDG).Count();

            
            if (diem == 0)
            {
                return PartialView("CanNhap");
            }
            else
            {   ViewBag.SiSo= db.BienBans
                .Where(dg => dg.MaDuGio == maDG )
                .Select(dg => dg.SiSo)
                .FirstOrDefault();
                return PartialView("ChiXem");
            }

        }
        public ActionResult XoaDuGio(FormCollection f)
        {
            try
            {
                int maDG = int.Parse(f["id"]);
                var dg = db.DuGios.SingleOrDefault(n => n.MaDuGio == maDG);
                db.DuGios.DeleteOnSubmit(dg);
                db.SubmitChanges();
                return View("XemDKDG");
            }
            catch
            {
                return View("XemDKDG");
            }

        }
        public ActionResult TenCT(int ma)
        {
            string truy = @"select MaPhieuCham, MaCT, TenCT from(
                SELECT dbo.PhieuCham.MaPhieuCham, dbo.DuGio.MaDK, dbo.DangKyThaoGiang.MaGV
                FROM     dbo.PhieuCham INNER JOIN
                                  dbo.DuGio ON dbo.PhieuCham.MaDuGio = dbo.DuGio.MaDuGio INNER JOIN
                                  dbo.DangKyThaoGiang ON dbo.DuGio.MaDK = dbo.DangKyThaoGiang.MaDK) A join

                (SELECT dbo.ChuongTrinh.MaCT, dbo.ChuongTrinh.TenCT, dbo.GiangVien.MaGV
                FROM     dbo.ChuongTrinh INNER JOIN
                                  dbo.GiangVien ON dbo.ChuongTrinh.MaCT = dbo.GiangVien.MaCT) B on a.MaGV= B.MaGV where MaPhieuCham="+ ma;

            var a = db.ExecuteQuery<TenCTQuaPhieu>(truy).ToList();
            
            return PartialView(a);
        }

        public ActionResult TenThuKy( int madk)
        {
            int k = int.Parse(Session["CapDo"].ToString());
            if (k==1)
            {
                var dgdong = db.DuGios
                .Where(dg => dg.MaDK == madk && dg.ThuKy == true)
                .FirstOrDefault();

                ViewBag.ten = dgdong.DangKyThaoGiang.GiangVien.HoTenGV.ToString();

            }
            else
            {
                ViewBag.ten = @Session["Ten"].ToString();
            }
            
            return PartialView();
        }
                 
        

    }
}