using System;
using System.Collections.Generic;
using System.Text;

namespace TelemetryLibrary
{
    public class Request
    {
        public double requestSize { get; set; }
        public double duration { get; set; }
        public double responseSize { get; set; }
        public int nbCalls { get; set; }

        public Dictionary<string, string> dictLabels = new Dictionary<string, string>();


    }
}
