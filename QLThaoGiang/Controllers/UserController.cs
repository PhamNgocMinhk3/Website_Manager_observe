using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Web.Mvc;
using QLThaoGiang.Models;
using System.Net;
using System.IO;
using System.Net.Mime;
namespace QLThaoGiang.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        private dbDataContext db = new dbDataContext();

        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(FormCollection f)
        {
            try
            {
                var ma = int.Parse(f["TenDN"]);
                var mk = f["MatKhau"];
                var kh = db.GiangViens.SingleOrDefault(n => n.MaGV == ma && n.MatKhau == mk);
                Session["Ten"] = kh.HoTenGV;
                Session["Ma"] = kh.MaGV;
                Session["TaiKhoan"] = kh;
                Session["CapDo"] = kh.MaCV;

                return RedirectToAction("DangSachThaoGiang", "DangKyThaoGiang");
            }
            catch
            {
                ViewBag.kt = "Thông tin của bạn không đúng vui lòng thực hiện lại!";
                return View();
            }
        }

        public ActionResult DangXuat()
        {
            Session["TaiKhoan"] = null;
            return View("DangNhap");
        }

        public ActionResult LayLaiMatKhau()
        {
            return View();
        }

        [HttpPost]
        public JsonResult LayLaiMatKhau(string mail)
        {
            try
            {
                string email = mail;
                var random = new Random();
                var password = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz0123456789", 6)
                                    .Select(s => s[random.Next(s.Length)]).ToArray());

                var user = db.GiangViens.SingleOrDefault(u => u.Email == email);
                if (user != null)
                {
                    user.MatKhau = password;
                    db.SubmitChanges();

                    var smtpClient = new SmtpClient("smtp.gmail.com", 587)
                    {
                        Credentials = new NetworkCredential("phamngocminhchemical@gmail.com", "xwgq gfmf rqzw nevb"),
                        EnableSsl = true
                    };

                    var message = new MailMessage();
                    message.From = new MailAddress("phamngocminhchemical@gmail.com");
                    message.To.Add(new MailAddress(email));
                    message.Subject = "Thông tin mật khẩu mới";

                    // Sử dụng HTML trong nội dung email
                    message.IsBodyHtml = true;
                    message.Body = $"<span style='font-size: larger;'>Mật khẩu mới của bạn là:</span> <strong>{password}</strong>";

                    smtpClient.Send(message);

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public ActionResult DoiMatKhau()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DoiMatKhau(string newPassword, string oldPassword)
        {
            try
            {
                // Lấy thông tin người dùng từ Session
                GiangVien b = (GiangVien)Session["Taikhoan"];
                if (b == null)
                {
                    // Nếu không tìm thấy thông tin người dùng trong Session, trả về mã lỗi 401
                    return Json(new { st = 401 });
                }

                int magv = b.MaGV;
                var mk = oldPassword.Trim();
                // Tìm kiếm GiangVien trong CSDL với mã và mật khẩu cũ
                var gv = db.GiangViens.SingleOrDefault(n => n.MaGV == magv && n.MatKhau == mk);

                if (gv == null)
                {
                    // Nếu không tìm thấy người dùng hoặc mật khẩu cũ không chính xác, trả về mã lỗi 500
                    return Json(new { st = 500 });
                }
                else
                {
                    // Cập nhật mật khẩu mới và lưu vào CSDL
                    gv.MatKhau = newPassword;
                    db.SubmitChanges(); // Sử dụng SaveChanges() thay vì SubmitChanges() nếu sử dụng Entity Framework

                    // Trả về mã thành công
                    return Json(new { st = 200 });
                }
            }
            catch 
            {
                // Bắt các ngoại lệ và trả về mã lỗi 500
                return Json(new { st = 500 });
            }
        }


        [AllowAnonymous]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Redirect to Google for authentication
            var redirectUri = Url.Action("ExternalLoginCallback", "User", new { ReturnUrl = returnUrl }, Request.Url.Scheme);
            var googleAuthenticationUrl = GetGoogleAuthenticationUrl(redirectUri);
            return Redirect(googleAuthenticationUrl);
        }

        private string GetGoogleAuthenticationUrl(string redirectUri)
        {
            // Replace with your actual Google API credentials
            var clientId = "";
            var scope = ""; // Adjust the scope as needed

            return $"";
        }

        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            var code = Request.QueryString["code"];
            if (code == null)
            {
                // Handle the error accordingly
                return RedirectToAction("Login");
            }

            var accessToken = GetGoogleAccessToken(code);
            var userDetails = GetGoogleUserDetails(accessToken);

            if (userDetails != null)
            {
                // Extract user details from the Google response
                string googleId = userDetails.id;
                string email = userDetails.email;
                string fullName = userDetails.name;

                // Check if the user with this Google ID already exists in the database
                GiangVien kh = db.GiangViens.SingleOrDefault(n => n.GoogleID == googleId);

                if (kh != null)
                {
                    // Log in the existing user
                    Session["Ten"] = kh.HoTenGV;
                    Session["Ma"] = kh.MaGV;
                    Session["TaiKhoan"] = kh;
                    Session["CapDo"] = kh.MaCV;

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Check if a user with the same email already exists
                    kh = db.GiangViens.SingleOrDefault(n => n.Email == email);

                    if (kh != null)
                    {
                        // Handle the case when a user with the same email already exists
                        // You may want to log in the existing user or display an error message
                        Session["Ten"] = kh.HoTenGV;
                        Session["Ma"] = kh.MaGV;
                        Session["TaiKhoan"] = kh;
                        Session["CapDo"] = kh.MaCV;

                        return RedirectToAction("DangSachThaoGiang", "DangKyThaoGiang");

                    }
                    return RedirectToAction("DangSachThaoGiang", "DangKyThaoGiang");

                }
            }

            // Handle the case when user details retrieval fails
            return RedirectToAction("Login");
        }


        private string GetGoogleAccessToken(string code)
        {// Replace with your actual Google API credentials
            var clientId = "";
            var clientSecret = "";
            var redirectUri = Url.Action("ExternalLoginCallback", "User", null, Request.Url.Scheme);

            var tokenUrl = "";
            var tokenRequestData = $"";

            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var response = client.UploadString(tokenUrl, "POST", tokenRequestData);
                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
                return result.access_token;
            }
        }

        private dynamic GetGoogleUserDetails(string accessToken)
        {
            var userInfoUrl = "";
            using (var client = new WebClient())
            {
                var json = client.DownloadString($"{userInfoUrl}?access_token={accessToken}");
                return Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            }
        }
        public JsonResult Xoatb(int matb)
        {
            try
            {
                int magv = (int)Session["Ma"];
                var tb = db.CT_ThongBaos.SingleOrDefault(n => n.MaTB==matb && n.NguoiNhan== magv);
                db.CT_ThongBaos.DeleteOnSubmit(tb);
                db.SubmitChanges();

                return Json(new { code = 200 }, JsonRequestBehavior.AllowGet); //thành công 
            }
            catch
            {
                return Json(new { code = 500 }, JsonRequestBehavior.AllowGet);// thất bại
            }
        }

    }
}