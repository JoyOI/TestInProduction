using System;
using Microsoft.AspNetCore.Mvc;

namespace JoyOI.TestInProduction.Controllers
{
    public class BaseController : Controller
    {
        [NonAction]
        public IActionResult Result(int code, string msg, object result = null)
        {
            HttpContext.Response.StatusCode = code;
            return Json(new { code = code, msg = msg, result = result });
        }
    }
}
