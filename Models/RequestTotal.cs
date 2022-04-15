using System;
using System.Collections.Generic;
using System.Text;

namespace TelemetryLibrary
{
    public class RequestTotal
    {
        private string strRequestMethod;
        public string ReqMethod
        {
            set { this.strRequestMethod = value; }
            get { return this.strRequestMethod; }
        }
    }
}
