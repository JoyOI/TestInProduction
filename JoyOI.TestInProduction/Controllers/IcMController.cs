using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using JoyOI.TestInProduction.Models;

namespace JoyOI.TestInProduction.Controllers
{
    public class IcMController : BaseController
    {
        public static HttpClient Client = new HttpClient() { BaseAddress = new Uri(Startup.Config["IcM:Url"]) };

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            using (var sr = new StreamReader(Request.Body))
            {
                var jsonText = await sr.ReadToEndAsync();
                var obj = JsonConvert.DeserializeObject<Incident>(jsonText);
                using (var response = await Client.PostAsync("/api/incident", new FormUrlEncodedContent(new Dictionary<string, string> {
                    { "title", obj.title },
                    { "body", obj.body },
                    { "severity", obj.severity.ToString() },
                    { "projectid", Startup.Config["IcM:ProjectId"] },
                    { "secret", Startup.Config["IcM:Secret"] }
                })))
                {
                    return Content(await response.Content.ReadAsStringAsync(), "application/json");
                }
            }
        }
    }
}
