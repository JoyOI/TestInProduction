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

        public async Task<IActionResult> LocalJudge()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri("http://api.oj.joyoi.cn") })
            using (var response = await client.PutAsync("/api/user/session", new StringContent(JsonConvert.SerializeObject(new
            {
                username = Startup.Config["OnlineJudge:Username"],
                password = Startup.Config["OnlineJudge:Password"]
            }))))
            {
                var result = JsonConvert.DeserializeObject<ApiResult<dynamic>>(await response.Content.ReadAsStringAsync());
                string cookie = result.data.cookie;
                client.DefaultRequestHeaders.Add("joyoi_cookie", cookie);
                using (var res = await client.PutAsync("/api/judge", new StringContent("{\r\n    \"problemId\": \"a-plus-b-problem\",\r\n    \"isSelfTest\": false,\r\n    \"code\": \"#include <stdio.h>\\r\\n\\r\\nint main()\\r\\n{\\r\\n    int a, b;\\r\\n    scanf(\\\"%d%d\\\", &a, &b);\\r\\n    printf(\\\"%d\\\\n\\\", a + b);\\r\\n}\",\r\n    \"language\": \"C++\",\r\n    \"data\": null,\r\n    \"contestId\": null\r\n}")))
                {
                    var judgeId = JsonConvert.DeserializeObject<ApiResult<Guid>>(await res.Content.ReadAsStringAsync()).data;
                    var retry = 20;
                    while(-- retry >= 0)
                    {
                        var ret = await PullResultAsync(client, judgeId);
                        if (ret == "Pending" || ret == "Running")
                        {
                            await Task.Delay(5000);
                        }
                        else if (ret != "Accepted")
                        {
                            await TriggerIncidentAsync(new Incident
                            {
                                title = "[OJ][评测功能][本地题库] 每小时评测测试发生异常",
                                severity = 3,
                                body = "在本次生产环境中测试时，提交的本地题库中的<A+B Problem>题目发生评测异常，预期结果为Accepted，实际结果为" + ret + "\r\n\r\n评测记录：[" + judgeId + "](http://joyoi.org/judge/" + judgeId + ")"
                            });
                        }
                        else
                        {
                            return Result(200, "Succeeded");
                        }
                    }

                    await TriggerIncidentAsync(new Incident
                    {
                        title = "[OJ][评测功能][本地题库] 每小时评测测试发生异常",
                        severity = 3,
                        body = "在本次生产环境中测试时，提交的本地题库中的<A+B Problem>题目发生评测异常，获取评测结果超时\r\n\r\n评测记录：[" + judgeId + "](http://joyoi.org/judge/" + judgeId + ")"
                    });

                    return Result(200, "Succeeded");
                }
            }
        }

        public async Task<IActionResult> BzojJudge()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri("http://api.oj.joyoi.cn") })
            using (var response = await client.PutAsync("/api/user/session", new StringContent(JsonConvert.SerializeObject(new
            {
                username = Startup.Config["OnlineJudge:Username"],
                password = Startup.Config["OnlineJudge:Password"]
            }))))
            {
                var result = JsonConvert.DeserializeObject<ApiResult<dynamic>>(await response.Content.ReadAsStringAsync());
                string cookie = result.data.cookie;
                client.DefaultRequestHeaders.Add("joyoi_cookie", cookie);
                using (var res = await client.PutAsync("/api/judge", new StringContent("{\r\n    \"problemId\": \"bzoj-1000\",\r\n    \"isSelfTest\": false,\r\n    \"code\": \"#include <stdio.h>\\r\\n\\r\\nint main()\\r\\n{\\r\\n    int a, b;\\r\\n    scanf(\\\"%d%d\\\", &a, &b);\\r\\n    printf(\\\"%d\\\\n\\\", a + b);\\r\\n}\",\r\n    \"language\": \"C++\",\r\n    \"data\": null,\r\n    \"contestId\": null\r\n}")))
                {
                    var judgeId = JsonConvert.DeserializeObject<ApiResult<Guid>>(await res.Content.ReadAsStringAsync()).data;
                    var retry = 20;
                    while (--retry >= 0)
                    {
                        var ret = await PullResultAsync(client, judgeId);
                        if (ret == "Pending" || ret == "Running")
                        {
                            await Task.Delay(5000);
                        }
                        else if (ret != "Accepted")
                        {
                            await TriggerIncidentAsync(new Incident
                            {
                                title = "[OJ][评测功能][BZOJ] 每小时评测测试发生异常",
                                severity = 4,
                                body = "在本次生产环境中测试时，提交的BZOJ题库中的<A+B Problem>题目发生评测异常，预期结果为Accepted，实际结果为" + ret + "\r\n\r\n评测记录：[" + judgeId + "](http://joyoi.org/judge/" + judgeId + ")"
                            });
                        }
                        else
                        {
                            return Result(200, "Succeeded");
                        }
                    }

                    await TriggerIncidentAsync(new Incident
                    {
                        title = "[OJ][评测功能][BZOJ] 每小时评测测试发生异常",
                        severity = 4,
                        body = "在本次生产环境中测试时，提交的BZOJ题库中的<A+B Problem>题目发生评测异常，获取评测结果超时\r\n\r\n评测记录：[" + judgeId + "](http://joyoi.org/judge/" + judgeId + ")"
                    });

                    return Result(200, "Succeeded");
                }
            }
        }

        public async Task<IActionResult> CodeVSJudge()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri("http://api.oj.joyoi.cn") })
            using (var response = await client.PutAsync("/api/user/session", new StringContent(JsonConvert.SerializeObject(new
            {
                username = Startup.Config["OnlineJudge:Username"],
                password = Startup.Config["OnlineJudge:Password"]
            }))))
            {
                var result = JsonConvert.DeserializeObject<ApiResult<dynamic>>(await response.Content.ReadAsStringAsync());
                string cookie = result.data.cookie;
                client.DefaultRequestHeaders.Add("joyoi_cookie", cookie);
                using (var res = await client.PutAsync("/api/judge", new StringContent("{\r\n    \"problemId\": \"codevs-1000\",\r\n    \"isSelfTest\": false,\r\n    \"code\": \"#include <stdio.h>\\r\\n\\r\\nint main()\\r\\n{\\r\\n    int a, b;\\r\\n    scanf(\\\"%d%d\\\", &a, &b);\\r\\n    printf(\\\"%d\\\\n\\\", a + b);\\r\\n}\",\r\n    \"language\": \"C++\",\r\n    \"data\": null,\r\n    \"contestId\": null\r\n}")))
                {
                    var judgeId = JsonConvert.DeserializeObject<ApiResult<Guid>>(await res.Content.ReadAsStringAsync()).data;
                    var retry = 20;
                    while (--retry >= 0)
                    {
                        var ret = await PullResultAsync(client, judgeId);
                        if (ret == "Pending" || ret == "Running")
                        {
                            await Task.Delay(5000);
                        }
                        else if (ret != "Accepted")
                        {
                            await TriggerIncidentAsync(new Incident
                            {
                                title = "[OJ][评测功能][CODEVS] 每小时评测测试发生异常",
                                severity = 4,
                                body = "在本次生产环境中测试时，提交的CODEVS题库中的<A+B Problem>题目发生评测异常，预期结果为Accepted，实际结果为" + ret + "\r\n\r\n评测记录：[" + judgeId + "](http://joyoi.org/judge/" + judgeId + ")"
                            });
                        }
                        else
                        {
                            return Result(200, "Succeeded");
                        }
                    }

                    await TriggerIncidentAsync(new Incident
                    {
                        title = "[OJ][评测功能][CODEVS] 每小时评测测试发生异常",
                        severity = 4,
                        body = "在本次生产环境中测试时，提交的CODEVS题库中的<A+B Problem>题目发生评测异常，获取评测结果超时\r\n\r\n评测记录：[" + judgeId + "](http://joyoi.org/judge/" + judgeId + ")"
                    });

                    return Result(200, "Succeeded");
                }
            }
        }

        private async Task<string> PullResultAsync(HttpClient client, Guid id)
        {
            using (var response = await client.GetAsync("/api/judge/" + id))
            {
                var result = JsonConvert.DeserializeObject<ApiResult<dynamic>>(await response.Content.ReadAsStringAsync());
                return result.data.result;
            }
        }
    }
}
