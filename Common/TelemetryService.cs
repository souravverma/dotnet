using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Caching;
using Microsoft.AspNetCore.Http;
using System.Runtime.InteropServices;


namespace TelemetryLibrary
{
    public class TelemetryService
    {
        private string strName = "";
        private string strPath = "";
        private IHttpContextAccessor Accessor;
        private const int DigitsInResult = 2;
        Metrics objMetric;

        Stopwatch stopwatch = new Stopwatch();
        public TelemetryService(string pname, string ppath, IHttpContextAccessor _accessor)
        {
            this.strName = pname;
            this.strPath = ppath;
            this.Accessor = _accessor;

        }

        public string getMetrics()
        {
            objMetric = calculateMetrics();
            string jsonData = objMetric.ToJSONRepresentation();
            return jsonData;
        }

        public Metrics calculateMetrics()
        {
            ObjectCache cache = MemoryCache.Default;
            objMetric = new Metrics();
            string strLabelName;
            stopwatch.Start();
            objMetric.CPU_usage = (int)GetOverallCpuUsagePercentage();

            if (IsUnix())
            {
                GetUnixMetrics();
            }
            else
            {
                GetWindowsMetrics();
            }

            objMetric.Sys_Uptime = (int)Environment.TickCount64/1000; // in seconds
            
            /***** for http_request_total *****/
            string strRequestMethod = Accessor.HttpContext.Request.Method;
            if (cache != null && cache.Contains("http_request_total") && cache.Get("http_request_total") != null)
            {
                cache.Set("http_request_total", (int)cache.Get("http_request_total") + 1, null);
                objMetric.Req_Total.Add(strRequestMethod, (int)cache.Get("http_request_total"));
            }
            else
            {
                cache.Add("http_request_total", 1, null);
                objMetric.Req_Total.Add(strRequestMethod, 1);
            }
            /***** end for http_request_total *****/

            // for response total
            if (cache != null && cache.Contains("http_response_total") && cache.Get("http_response_total") != null)
            {
                cache.Set("http_response_total", (int)cache.Get("http_response_total") + 1, null);
                objMetric.ResponseTotal = (int)cache.Get("http_response_total");
            }
            else
            {
                cache.Add("http_response_total", 1, null);
                objMetric.ResponseTotal = 1;
            }

            /******** for http_response_success_total & http_response_error_total ******/
            int intResStatusCode = Accessor.HttpContext.Response.StatusCode;
            if (intResStatusCode >= 400)
            {
                strLabelName = "error_" + intResStatusCode;
                if (cache != null && cache.Contains(strLabelName))
                {
                    cache.Set(strLabelName, (int)cache.Get(strLabelName) + 1, null);
                    objMetric.Res_Error.Add(strLabelName, (int)cache.Get(strLabelName));
                }
                else
                {
                    cache.Add(strLabelName, 1, null);
                    objMetric.Res_Error.Add(strLabelName, 1);
                }
            }
            else
            {
                strLabelName = "success_" + intResStatusCode;
                if (cache != null && cache.Contains(strLabelName))
                {
                    cache.Set(strLabelName, (int)cache.Get(strLabelName) + 1, null);
                    objMetric.Res_Success.Add(strLabelName, (int)cache.Get(strLabelName));
                }
                else
                {
                    cache.Add(strLabelName, 1, null);
                    objMetric.Res_Success.Add(strLabelName, 1);
                }
            }
            Request objRequest = new Request();
            objRequest.dictLabels.Add("method", "GET");
            if (intResStatusCode >= 400)
            {
                strLabelName = "error_" + intResStatusCode;
                if (cache != null && cache.Contains("Dict:" + strLabelName + "-" + strPath))
                {
                    cache.Set("Dict:" + strLabelName + "-" + strPath, (int)cache.Get("Dict:" + strLabelName + "-" + strPath) + 1, null);
                }
                else
                {
                    cache.Add("Dict:" + strLabelName + "-" + strPath, 1, null);
                }
            }
            else
            {
                strLabelName = "success_" + intResStatusCode;
                if (cache != null && cache.Contains("Dict:" + strLabelName + "-" + strPath))
                {
                    cache.Set("Dict:" + strLabelName + "-" + strPath, (int)cache.Get("Dict:" + strLabelName + "-" + strPath) + 1, null);
                }
                else
                {
                    cache.Add("Dict:" + strLabelName + "-" + strPath, 1, null);
                }
            }
            /**************** for Requests *************************/
            stopwatch.Stop();
            double duration = (double)(stopwatch.Elapsed).TotalSeconds;
            double reqDuration;
            double requestSize = 0.0;
            if (Accessor.HttpContext.Request.ContentLength.HasValue)
            {
                requestSize = (double)Accessor.HttpContext.Request.ContentLength;
            }

            if (cache != null && cache.Contains("REQUESTS_" + this.strPath))
            {
                if (duration == double.Parse(cache.Get("REQUESTS_" + this.strPath + "_duration").ToString()))
                {
                    reqDuration = double.Parse(cache.Get("REQUESTS_" + this.strPath + "_duration").ToString());
                }
                else
                {
                    reqDuration = duration;
                }
                objMetric.DictRequest[this.strPath] = new List<Request>();
                objMetric.DictRequest[this.strPath].Add(new Request()
                {
                    requestSize = requestSize,
                    duration = double.Parse(reqDuration.ToString()),
                    responseSize = 0,
                    nbCalls = (int)cache.Get("REQUESTS_" + this.strPath + "_nbCalls") + 1
                }); 
                cache.Set("REQUESTS_" + this.strPath + "_nbCalls", (int)cache.Get("REQUESTS_" + this.strPath + "_nbCalls") + 1, null);
                cache.Set("REQUESTS_list:" + this.strPath, objMetric.DictRequest[this.strPath], null);
                cache.Set("REQUESTS_" + this.strPath + "_duration", reqDuration, null);
            }
            else
            {
                objMetric.DictRequest[this.strPath] = new List<Request>();
                objMetric.DictRequest[this.strPath].Add(new Request()
                {
                    requestSize = requestSize,
                    duration = double.Parse(duration.ToString()),
                    responseSize = 0,
                    nbCalls = 1
                });
                cache.Add("REQUESTS_" + this.strPath, 1, null);
                cache.Add("REQUESTS_" + this.strPath + "_nbCalls", 1, null);
                cache.Add("REQUESTS_" + this.strPath + "_duration", duration, null);
                cache.Add("REQUESTS_list:" + this.strPath, objMetric.DictRequest[this.strPath], null);
            }

            /****************************************************************/

            foreach (var item in cache)
            {
                if (item.Key.Contains("list:"))
                {
                    int a = item.Key.IndexOf(":");
                    string s = item.Key.Substring(a + 1);
                    objMetric.DictRequest[s] = (List<Request>)item.Value;
                }

                if (item.Key.Contains("Dict:"))
                {
                    int start = item.Key.IndexOf(":") + 1;
                    objMetric.Statuses.Add(item.Key.Substring(start), (int)item.Value);
                }
            }
            objMetric.Labels.Add("Method", "GET");
            objMetric.OptionName = this.strName;
            return objMetric;
        }

        private int GetOverallCpuUsagePercentage()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            Task.Delay(500);
            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return (int)cpuUsageTotal * 100;
        }
        private bool IsUnix()
        {
            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            return isUnix;
        }

        private void GetWindowsMetrics()
        {
            var output = "";
            double Total;
            var info = new ProcessStartInfo();
            info.FileName = "wmic";
            info.Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            var lines = output.Trim().Split("\n");
            var freeMemoryParts = lines[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
            var totalMemoryParts = lines[1].Split("=", StringSplitOptions.RemoveEmptyEntries);
            Total = Math.Round(double.Parse(totalMemoryParts[1]) / 1024, 0);
            this.objMetric.Mem_free = Math.Round(double.Parse(freeMemoryParts[1]) / 1024, 0);
            this.objMetric.Mem_used = Total - this.objMetric.Mem_free;
            
        }

        private void GetUnixMetrics()
        {
            var output = "";
            var info = new ProcessStartInfo("free -m");
            info.FileName = "/bin/bash";
            info.Arguments = "-c \"free -m\"";
            info.RedirectStandardOutput = true;

            using (var process = Process.Start(info))
            {
                output = process.StandardOutput.ReadToEnd();
            }
            var lines = output.Split("\n");
            var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);

            this.objMetric.Mem_free = double.Parse(memory[2]);
            this.objMetric.Mem_used = double.Parse(memory[3]);
        }
    }
}
