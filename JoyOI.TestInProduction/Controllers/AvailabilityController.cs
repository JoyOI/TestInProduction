using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JoyOI.TestInProduction.Controllers
{
    public class AvailabilityController : Controller
    {
        public IActionResult OnlineJudge()
        {
            return View();
        }
    }
}
