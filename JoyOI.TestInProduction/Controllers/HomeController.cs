using Microsoft.AspNetCore.Mvc;

namespace JoyOI.TestInProduction.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Content("Joy OI 在生产环境中测试 运行正常");
        }
    }
}
