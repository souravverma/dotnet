using System;
using System.Collections.Generic;
using System.Text;

namespace TelemetryLibrary
{
   public class ResponseError
    {
        private string strLabelErrorName;

        public ResponseError(string label)
        {
            this.strLabelErrorName = label;
        }
    }
}
