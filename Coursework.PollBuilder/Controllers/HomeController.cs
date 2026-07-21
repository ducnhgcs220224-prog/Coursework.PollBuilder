using Microsoft.AspNetCore.Mvc;

namespace Coursework.PollBuilder.Controllers
{
    public class HomeController : Controller
    {
        // 1. Trang chủ (Tạo Poll)
        public IActionResult Index()
        {
            return View();
        }

        // 2. Trang Bình chọn
        [Route("poll/{code}")]
        public IActionResult Vote(string code)
        {
            ViewBag.PollCode = code; // Truyền mã code sang giao diện
            return View();
        }

        // 3. Trang Xem Kết quả
        [Route("poll/{code}/results")]
        public IActionResult Results(string code)
        {
            ViewBag.PollCode = code;
            return View();
        }
    }
}