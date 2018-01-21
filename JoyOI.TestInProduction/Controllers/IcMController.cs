using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using JoyOI.TestInProduction.Models;

namespace JoyOI.TestInProduction.Controllers
{
    public class IcMController : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> Index()
        {
            using (var sr = new StreamReader(Request.Body))
            {
                var jsonText = await sr.ReadToEndAsync();
                var obj = JsonConvert.DeserializeObject<Incident>(jsonText);
                return Content(await TriggerIncidentAsync(obj), "application/json");
            }
        }
    }
}
