using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using JoyOI.TestInProduction.Models;

namespace JoyOI.TestInProduction.Controllers
{
    public class BaseController : Controller
    {
        private static HttpClient IcMClient = new HttpClient() { BaseAddress = new Uri(Startup.Config["IcM:Url"]) };
        
        protected IActionResult Result(int code, string msg, object result = null)
        {
            HttpContext.Response.StatusCode = code;
            return Json(new { code = code, msg = msg, result = result });
        }
        
        protected async Task<string> TriggerIncidentAsync(Incident obj)
        {
            using (var sr = new StreamReader(Request.Body))
            {
                using (var response = await IcMClient.PostAsync("/api/incident", new FormUrlEncodedContent(new Dictionary<string, string> {
                    { "title", obj.title },
                    { "body", obj.body },
                    { "severity", obj.severity.ToString() },
                    { "projectid", Startup.Config["IcM:ProjectId"] },
                    { "secret", Startup.Config["IcM:Secret"] }
                })))
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
