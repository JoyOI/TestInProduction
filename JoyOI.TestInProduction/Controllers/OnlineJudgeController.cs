using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using JoyOI.OnlineJudge.Models;
using JoyOI.TestInProduction.Models;

namespace JoyOI.TestInProduction.Controllers
{
    public class OnlineJudgeController : BaseController
    {
        public async Task<IActionResult> SystemError()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri("http://api.oj.joyoi.cn") })
            using (var response = await client.GetAsync($"/api/judge/all?problemid=&status=8&userId=&language=&begin={HttpUtility.UrlEncode(DateTime.UtcNow.AddHours(-1).ToString())}&end=&page=1"))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<ApiResult<PagedResult<JudgeStatus>>>(await response.Content.ReadAsStringAsync());
                    if (result.data.result.Count() > 5)
                    {
                        await TriggerIncidentAsync(new Incident
                        {
                            severity = 2,
                            title = "[OJ][评测功能] 1小时内发生5次以上System Error",
                            body = "在1个小时内，下列的评测记录发生了System Error：\r\n\r\n" + string.Join("\r\n\r\n", result.data.result.Select(x => $"[{x.Id}](http://joyoi.org/judge/{x.Id})")) 
                        });
                    }
                    return Result(200, "Succeeded");
                }
                else
                {
                    return Result(500, "Failed");
                }
            }
        }
        public async Task<IActionResult> Pending()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri("http://api.oj.joyoi.cn") })
            using (var response = await client.GetAsync($"/api/judge/all?problemid=&status=11&userId=&language=&begin={HttpUtility.UrlEncode(DateTime.UtcNow.AddHours(-2).ToString())}&end={HttpUtility.UrlEncode(DateTime.UtcNow.AddHours(-1).ToString())}&page=1"))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<ApiResult<PagedResult<JudgeStatus>>>(await response.Content.ReadAsStringAsync());
                    if (result.data.result.Count() > 0)
                    {
                        await TriggerIncidentAsync(new Incident
                        {
                            severity = 2,
                            title = "[OJ][评测功能] 评测记录超过1小时处于Pending状态",
                            body = "下列的评测记录处于Pending状态超过1小时：\r\n\r\n" + string.Join("\r\n\r\n", result.data.result.Select(x => $"[{x.Id}](http://joyoi.org/judge/{x.Id})"))
                        });
                    }
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
