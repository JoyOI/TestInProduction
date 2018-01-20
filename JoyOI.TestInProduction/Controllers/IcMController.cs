using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace JoyOI.TestInProduction.Controllers
{
    public class IcMController : BaseController
    {
        [HttpPost]
        public IActionResult Index()
        {
            return View();
        }
    }
}
