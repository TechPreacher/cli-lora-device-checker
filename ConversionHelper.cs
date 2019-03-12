using System;
using System.Collections.Generic;
using System.Text;

namespace cli_lora_device_checker
{
    public static class ConversionHelper
    {
        public static string CleanString(string inString)
        {
            var outString = inString.Replace("\'", string.Empty);

            return outString;
        }
    }
}
