using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace TelemetryLibrary
{
    public class Metrics
    {
        private double system_cpu_usage = 0;
        private double system_mem_used = 0;
        private double system_mem_free = 0;
        private int system_uptime = 0;
        private string labelName;
        private int labelValue;
        private string optionName;
        private int intResponseTotal;

        public Dictionary<string, int> http_request_total = new Dictionary<string, int>();
        public Dictionary<string, int> http_response_error_total = new Dictionary<string, int>();
        public Dictionary<string, int> http_response_success_total = new Dictionary<string, int>();
        public Dictionary<string, List<Request>> requests = new Dictionary<string, List<Request>>();
        public Dictionary<string, string> dictLabels = new Dictionary<string, string>();
        public Dictionary<string, int> dictStatuses = new Dictionary<string, int>();
        public double CPU_usage
        {
            set { this.system_cpu_usage = value; }
            get { return this.system_cpu_usage; }
        }

        public double Mem_used
        {
            set { this.system_mem_used = value; }
            get { return this.system_mem_used; }
        }

        public double Mem_free
        {
            set { this.system_mem_free = value; }
            get { return this.system_mem_free; }
        }

        public int Sys_Uptime
        {
            set { this.system_uptime = value; }
            get { return this.system_uptime; }
        }

        public Dictionary<string, int> Req_Total
        {
            set { this.http_request_total = value; }
            get { return this.http_request_total; }
        }
        public int ResponseTotal
        {
            set { this.intResponseTotal = value; }
            get { return this.intResponseTotal; }
        }

        public Dictionary<string, int> Res_Error
        {
            set { this.http_response_error_total = value; }
            get { return this.http_response_error_total; }
        }
        public Dictionary<string, int> Res_Success
        {
            set { this.http_response_success_total = value; }
            get { return this.http_response_success_total; }
        }

        public Dictionary<string, List<Request>> DictRequest
        {
            set { this.requests = value; }
            get { return this.requests; }
        }
        public string LabelName
        {
            set { this.labelName = value; }
            get { return this.labelName; }
        }
        public int LabelValue
        {
            set { this.labelValue = value; }
            get { return this.labelValue; }
        }
        public string OptionName
        {
            set { this.optionName = value; }
            get { return this.optionName; }
        }
        public Dictionary<string, string> Labels
        {
            set { this.dictLabels = value; }
            get { return this.dictLabels; }
        }
        public Dictionary<string, int> Statuses
        {
            set { this.dictStatuses = value; }
            get { return this.dictStatuses; }
        }

        public String ToJSONRepresentation()
        {
            StringBuilder sb = new StringBuilder();
            JsonWriter jw = new JsonTextWriter(new StringWriter(sb));
            jw.Formatting = Formatting.Indented;
            jw.WriteStartObject();
            jw.WritePropertyName("name");
            jw.WriteValue(this.OptionName);
            jw.WritePropertyName("system_cpu_usage");
            jw.WriteValue(this.system_cpu_usage);
            jw.WritePropertyName("system_mem_used");
            jw.WriteValue(this.system_mem_used);
            jw.WritePropertyName("system_mem_free");
            jw.WriteValue(this.system_mem_free);
            jw.WritePropertyName("system_uptime");
            jw.WriteValue(this.system_uptime);
            jw.WritePropertyName("http_response_total");
            jw.WriteValue(this.intResponseTotal);

            /***** for http_request_total:  ****/
            jw.WritePropertyName("http_request_total");
            jw.WriteStartObject();

            foreach (var dictRequestTotal in http_request_total.Keys.ToArray())
            {
                var key = dictRequestTotal;
                jw.WritePropertyName(key);
                jw.WriteValue(http_request_total[dictRequestTotal]);
            }

            jw.WriteEndObject();

            /****************** end for http_request_total:  ****/

            /***** for http_response_success_total:  ****/
            jw.WritePropertyName("http_response_success_total");
            jw.WriteStartObject();

            foreach (var dictResponseSuccessTotalkey in http_response_success_total.Keys.ToArray())
            {
                jw.WritePropertyName(dictResponseSuccessTotalkey);
                jw.WriteValue(http_response_success_total[dictResponseSuccessTotalkey]);
            }

            jw.WriteEndObject();
            /***** end for http_response_success_total ****/

            /***** for http_response_error_total:  ****/
            jw.WritePropertyName("http_response_error_total");
            jw.WriteStartObject();

            foreach (var dictkey in http_response_error_total.Keys.ToArray())
            {
                var key = dictkey;
                jw.WritePropertyName(key);
                jw.WriteValue(http_response_error_total[dictkey]);
            }

            jw.WriteEndObject();
            /***** end for http_response_error_total ****/

            jw.WritePropertyName("requests");
            jw.WriteStartObject();

            StringBuilder sbrequest = new StringBuilder();
            JsonWriter jwrequest = new JsonTextWriter(new StringWriter(sbrequest));

            foreach (var dictkeyRequest in requests.Keys.ToArray())
            {
                if (sbrequest.Length != 0)
                {
                    sbrequest.Append(',');
                }
                var key = dictkeyRequest;
                if (key != "requests")
                {
                    jwrequest.WritePropertyName(key);
                    jwrequest.WriteStartObject();
                }
                foreach (var item in requests[key])
                {
                    foreach (var prop in item.GetType().GetProperties())
                    {
                        jwrequest.WritePropertyName(prop.Name);
                        jwrequest.WriteValue(prop.GetValue(item, null));
                    }
                }
                /***** For Labels*******/

                if (this.Labels.ContainsKey("Method"))
                {
                    jwrequest.WritePropertyName("labels");
                    jwrequest.WriteStartObject();
                    foreach (var labelkey in this.Labels.Keys.ToArray())
                    {
                        jwrequest.WritePropertyName(labelkey);
                        jwrequest.WriteValue(this.Labels[labelkey]);
                    }
                    jwrequest.WriteEndObject();
                }

                /************************/

                /***** For Labels*******/
                jwrequest.WritePropertyName("statuses");
                jwrequest.WriteStartObject();

                foreach (var dictStatus in this.Statuses.Keys.ToArray())
                {
                    var label = dictStatus.Substring(dictStatus.LastIndexOf('-') + 1);
                    if (label == key)
                    {
                        var statusKey = dictStatus;
                        jwrequest.WritePropertyName(statusKey.Substring(0, statusKey.LastIndexOf('-')));
                        jwrequest.WriteValue(this.Statuses[statusKey]);
                    }
                }
                jwrequest.WriteEndObject();

                /************************/
                if (key != "requests")
                {
                    jwrequest.WriteEndObject();
                }
                sbrequest.Replace("\"responseSize\":0.0", "\"responseSize\": " + ((double)sbrequest.Length / 1024));
            }
            sb.Append(sbrequest);
            jw.WriteEndObject();
            /**********************************/

            foreach (var dictResponseSuccessTotalkey in http_response_success_total.Keys.ToArray())
            {
                jw.WritePropertyName(dictResponseSuccessTotalkey);
                jw.WriteValue(http_response_success_total[dictResponseSuccessTotalkey]);
            }

            foreach (var dictkey in http_response_error_total.Keys.ToArray())
            {
                var key = dictkey;
                jw.WritePropertyName(key);
                jw.WriteValue(http_response_error_total[dictkey]);
            }
            /**********************************/

            jw.WriteEndObject();
            return sb.ToString();
        }
    }
}
