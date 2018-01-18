using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace JoyOI.TestInProduction.Controllers
{
    public class AvailabilityController : BaseController
    {
        public async Task<IActionResult> OnlineJudge()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri("http://www.joyoi.cn") })
            using (var response = await client.GetAsync("/"))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Result(200, "Succeeded");
                }
                else
                {
                    return Result(500, "Failed");
                }
            }
        }
    }
}
