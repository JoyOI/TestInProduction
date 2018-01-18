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

        public async Task<IActionResult> Help()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri("http://help.joyoi.cn") })
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

        public async Task<IActionResult> Forum()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri("http://forum.joyoi.cn") })
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

        public async Task<IActionResult> UserCenter()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri("http://uc.joyoi.cn") })
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

        public async Task<IActionResult> OnlineJudgeAPI()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri("http://api.oj.joyoi.cn") })
            using (var response = await client.GetAsync("/api/problem/a-plus-b-problem"))
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
