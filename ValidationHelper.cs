namespace cli_lora_device_checker
{
    using System;

    public static class ValidationHelper
    {
        public static string CleanString(string inString)
        {
            string outString = null;

            if (!string.IsNullOrEmpty(inString))
            { 
                outString = inString.Replace("\'", string.Empty);
            }

            return outString;
        }

        public static bool ValidateKey(string hexString, int byteCount, out string error)
        {
            error = string.Empty;

            // hexString not dividable by 2.
            if (hexString.Length % 2 > 0)
            {
                error = "Hex string must contain an even number of characters";
                return false;
            }

            // hexString doesn't contain byteCount bytes.
            if (hexString.Length / 2 != byteCount)
            {
                error = $"Hex string doesn't contain the expected number of {byteCount} bytes";
                return false;
            }

            // Verify each individual byte for validity.
            for (int i = 0; i < hexString.Length; i += 2)
            {
                if (!int.TryParse(hexString.Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out _))
                {
                    error = $"Hex string contains invalid byte {hexString.Substring(i, 2)}";
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateSensorDecoder(string sensorDecoder, out string error)
        {
            bool isValid = true;
            error = string.Empty;

            if (string.IsNullOrEmpty(sensorDecoder))
            {
                error += "Informationi: SensorDecoder is empty. No decoder will be used. ";
                return isValid;
            }

            if (sensorDecoder.StartsWith("http") || sensorDecoder.Contains('/'))
            {
                if (!Uri.TryCreate(sensorDecoder, UriKind.Absolute, out Uri validatedUri))
                {
                    error += "Error: SensorDecoder has invalid URL. ";
                    isValid = false;
                }
                // if (validatedUri.Host.Any(char.IsUpper))
                if (!sensorDecoder.Contains(validatedUri.Host))
                {
                    error += "Error: SensorDecoder Hostname must be all lowercase. ";
                    isValid = false;
                }
                if (validatedUri.AbsolutePath.IndexOf("/api/") < 0)
                {
                    error += "Error: SensorDecoder is missing \"api\" keyword. ";
                    isValid = false;
                }
            }
            if (!isValid)
            {
                error += ("\nMake sure the URI based SensorDecoder Twin desired property looks like \"http://containername/api/decodername\".");
            }

            return isValid;
        }
    }
}
