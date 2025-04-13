using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLThaoGiang.Models;
using Spire.Doc;

namespace QLThaoGiang.Controllers
{
    public class XuatWordController : Controller
    {
        dbDataContext db = new dbDataContext();
        // GET: XuatWord
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ConvertToWord(string htmlContent)
        {
            // Tạo đường dẫn đến tệp Word
            string outputPath = Server.MapPath("~/Content/Output.docx");

            // Tạo một tài liệu Word mới
            Document document = new Document();

            // Thêm nội dung HTML vào tài liệu Word
            Section section = document.AddSection();
            section.AddParagraph().AppendHTML(htmlContent);

            // Lưu tài liệu Word
            document.SaveToFile(outputPath, FileFormat.Docx);

            // Đọc tệp thành mảng byte
            byte[] fileBytes = System.IO.File.ReadAllBytes(outputPath);

            // Xóa tệp sau khi đã gửi nó về client
            System.IO.File.Delete(outputPath);

            // Trả về tệp Word dưới dạng file để tải xuống
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Output.docx");
        }
        

    }
}