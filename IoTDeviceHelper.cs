using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace cli_lora_device_checker
{
    public class IoTDeviceHelper
    {
        public async Task<JObject> QueryTwinSingle(string devEui, ConfigurationHelper configurationHelper)
        {
            Twin twin;
            JObject twinData = null;

            try
            {
                twin = await configurationHelper.RegistryManager.GetTwinAsync(devEui);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }

            if (twin != null)
            {
                twinData = JObject.Parse(twin.Properties.Desired.ToJson());
                twinData.Remove("$metadata");
                twinData.Remove("$version");
            }

            return twinData;
        }

        public bool VerifyTwinSingle(string devEui, JObject twinData)
        {
            var error = String.Empty;

            bool isOTAA = false;
            bool isABP = false;
            bool isValid = true;

            var appEUI = (string)twinData["AppEUI"];
            var appKey = (string)twinData["AppKey"];

            var nwkSKey = (string)twinData["NwkSKey"];
            var appSKey = (string)twinData["AppSKey"];
            var devAddr = (string)twinData["DevAddr"];

            var GatewayID = (string)twinData["GatewayID"];
            var sensorDecoder = (string)twinData["SensorDecoder"];
            var classType = (string)twinData["ClassType"];

            if (!string.IsNullOrEmpty(appEUI) || !string.IsNullOrEmpty(appKey))
                isABP = true;

            if (!string.IsNullOrEmpty(nwkSKey) || !string.IsNullOrEmpty(appSKey) || !string.IsNullOrEmpty(devAddr))
                isOTAA = true;

            if (isABP && !isOTAA)
            {
                Console.WriteLine("ABP device configuration detected.");
                isValid = ValidateAbpDevice(appEUI, appKey, sensorDecoder, classType, out error);
            }

            else if (isOTAA && !isABP)
            {
                Console.WriteLine("OTAA device configuration detected.");
                isValid = ValidateOtaaDevice(nwkSKey, appSKey, devAddr, sensorDecoder, classType, out error);
            }

            else
            {
                Console.WriteLine("Error in configuration: Can't determine if OTAA or ABP device.");
                isValid = false;
            }

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine(error);
            }

            if (isValid)
            {
                Console.WriteLine($"The configuration for device {devEui} is valid.");
            }
            else
            {
                Console.WriteLine($"Error: The configuration for device {devEui} is NOT valid.");
            }

            return isValid;
        }

        public bool ValidateAbpDevice(string appEUI, string appKey, string sensorDecoder, string classType, out string error)
        {
            error = string.Empty;
            var validationError = string.Empty;
            bool isValid = true;

            if (string.IsNullOrEmpty(appEUI))
            {
                error = "Error in configuration: Missing Twin desired property: AppEUI \n";
                isValid = false;
            }
            if (ValidateKey(appEUI, 16, out validationError))
            {
                error = $"Error in configuration: AppEUI is invalid. {validationError} \n";
                isValid = false;
            }

            if (string.IsNullOrEmpty(appKey))
            {
                error = "Error in configuration: Missing Twin desired property: AppKey \n";
                isValid = false;
            }
            if (ValidateKey(appKey, 16, out validationError))
            {
                error = $"Error in configuration: AppKey is invalid. {validationError} \n";
                isValid = false;
            }

            if (!string.IsNullOrEmpty(sensorDecoder))
            {
                isValid = ValidateSensorDecoder(sensorDecoder, out validationError);
                error += validationError;
            }

            if (!string.IsNullOrEmpty(classType))
            {
                if (!string.Equals(classType, "C", StringComparison.CurrentCultureIgnoreCase))
                {
                    isValid = false;
                    error += "Error in configuration: If Twin desired property \"ClassType\" is set, it needs to be \"C\".";
                }
            }

            if (!string.IsNullOrEmpty(error))
                error = error.Substring(0, error.Length - 2);

            return isValid;
        }

        public bool ValidateOtaaDevice(string nwkSKey, string appSKey, string devAddr, string sensorDecoder, string classType, out string error)
        {
            error = string.Empty;
            var validationError = string.Empty;
            bool isValid = true;
            
            if (string.IsNullOrEmpty(nwkSKey))
            {
                error += "Error in configuration: Missing Twin desired property: NwkSKey. \n";
                isValid = false;
            }
            if (ValidateKey(nwkSKey, 16, out validationError))
            {
                error += $"Error in configuration: NwkSKey is invalid. {validationError} \n";
                isValid = false;
            }

            if (string.IsNullOrEmpty(appSKey))
            {
                error += "Error in configuration: Missing Twin desired property: AppSKey \n";
                isValid = false;
            }
            if (ValidateKey(appSKey, 16, out validationError))
            {
                error += $"Error in configuration: AppSKey is invalid. {validationError} \n";
                isValid = false;
            }

            if (string.IsNullOrEmpty(devAddr))
            {
                error += "Error in configuration: Missing Twin desired property: DevAddr \n";
                isValid = false;
            }
            if (ValidateKey(devAddr, 4, out validationError))
            {
                error += $"Error in configuration: DevAddr is invalid. {validationError} \n";
                isValid = false;
            }

            if (!string.IsNullOrEmpty(sensorDecoder))
            {
                isValid = ValidateSensorDecoder(sensorDecoder, out validationError);
                error += validationError;
            }

            if (!string.IsNullOrEmpty(classType))
            {
                if (!string.Equals(classType, "C", StringComparison.CurrentCultureIgnoreCase))
                {
                    isValid = false;
                    error += "Error in configuration: If Twin desired property \"ClassType\" is set, it needs to be \"C\".";
                }
            }

            if (!string.IsNullOrEmpty(error))
                error = error.Substring(0, error.Length-2);

            return isValid;
        }

        private bool ValidateKey(string hexString, int byteCount, out string error)
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

        public bool ValidateSensorDecoder(string sensorDecoder, out string error)
        {
            bool isValid = true;
            error = string.Empty;

            if (string.IsNullOrEmpty(sensorDecoder))
            {
                error += "Warning: SensorDecoder Twin desired property is empty. No decoder will be used. \n";
            }

            if (sensorDecoder.StartsWith("http"))
            {
                if (!Uri.TryCreate(sensorDecoder, UriKind.Absolute, out Uri validatedUri))
                {
                    error += "Invalid URL in URI based SensorDecoder Twin desired property. \n";
                    isValid = false;
                }
                if (validatedUri.Host.Any(char.IsUpper))
                {
                    error += "Hostname should be all lowercase in URI based SensorDecoder Twin desired property. \n";
                    isValid = false;
                }
                if (validatedUri.AbsolutePath.IndexOf("/api/") < 0)
                {
                    error += "\"api\" keyword is missing in URI based SensorDecoder Twin desired property. \n";
                    isValid = false;
                }
            }
            if (!isValid)
            {
                error += ("Make sure the URI based SensorDecoder Twin desired property looks like \"http://containername/api/decodername\". \n");
            }

            return isValid;
        }

        public async Task<bool> AddAbpDevice(Program.AddAbpOptions opts, ConfigurationHelper configurationHelper)
        {
            if (!configurationHelper.ReadConfig())
                return false;

            Console.WriteLine($"Adding ABP device to IoT Hub: {ConversionHelper.CleanString(opts.DevEui)} ...");

            bool isSuccess = false;
            var twinProperties = new TwinProperties();

            if (string.IsNullOrEmpty(opts.AppEui))
            {
                opts.AppEui = Keygen.Generate(16);
            }
            twinProperties.Desired["AppEUI"] = ConversionHelper.CleanString(opts.AppEui);

            if (string.IsNullOrEmpty(opts.AppKey))
            {
                opts.AppKey = Keygen.Generate(16);
            }
            twinProperties.Desired["AppKey"] = ConversionHelper.CleanString(opts.AppKey);

            twinProperties.Desired["GatewayID"] = ConversionHelper.CleanString(opts.GatewayId);

            twinProperties.Desired["SensorDecoder"] = ConversionHelper.CleanString(opts.SensorDecoder);

            if (!string.IsNullOrEmpty(opts.ClassType))
            {
                twinProperties.Desired["ClassType"] = ConversionHelper.CleanString(opts.ClassType);
            }

            var twin = new Twin();
            twin.Properties = twinProperties;

            var device = new Device(opts.DevEui);

            BulkRegistryOperationResult result;

            try
            {
                result = await configurationHelper.RegistryManager.AddDeviceWithTwinAsync(device, twin);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }

            if (result.IsSuccessful)
            {
                Console.WriteLine($"Done!");
                isSuccess = true;
            }
            else
            {
                Console.WriteLine($"Error adding device:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Device Id: {error.DeviceId}, Code: {error.ErrorCode}, Error: {error.ErrorStatus}");
                }
                isSuccess = false;
            }
            return isSuccess;
        }

        public async Task<bool> AddOtaaDevice(Program.AddOtaaOptions opts, ConfigurationHelper configurationHelper)
        {
            Console.WriteLine($"Adding OTAA device to IoT Hub: {ConversionHelper.CleanString(opts.DevEui)} ...");

            bool isSuccess = false;
            var twinProperties = new TwinProperties();

            if (string.IsNullOrEmpty(opts.AppSKey))
            {
                opts.AppSKey = Keygen.Generate(16);
            }
            twinProperties.Desired["AppSKey"] = ConversionHelper.CleanString(opts.AppSKey);

            if (string.IsNullOrEmpty(opts.NwkSKey))
            {
                opts.NwkSKey = Keygen.Generate(16);
            }
            twinProperties.Desired["NwkSKey"] = ConversionHelper.CleanString(opts.NwkSKey);

            if (string.IsNullOrEmpty(opts.NwkSKey))
            {
                opts.NwkSKey = Keygen.Generate(4);
            }
            twinProperties.Desired["DevAddr"] = opts.DevAddr;

            twinProperties.Desired["GatewayID"] = opts.GatewayId;

            twinProperties.Desired["SensorDecoder"] = opts.SensorDecoder;

            if (!string.IsNullOrEmpty(opts.ClassType))
            {
                twinProperties.Desired["ClassType"] = ConversionHelper.CleanString(opts.ClassType);
            }

            var twin = new Twin();
            twin.Properties = twinProperties;

            var device = new Device(opts.DevEui);

            BulkRegistryOperationResult result;

            try
            {
                result = await configurationHelper.RegistryManager.AddDeviceWithTwinAsync(device, twin);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }

            if (result.IsSuccessful)
            {
                Console.WriteLine($"Done!");
                isSuccess = true;
            }
            else
            {
                Console.WriteLine($"Error adding device:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Device Id: {error.DeviceId}, Code: {error.ErrorCode}, Error: {error.ErrorStatus}");
                }
                isSuccess = false;
            }
            return isSuccess;
        }

        public async Task QueryDevices(ConfigurationHelper configurationHelper)
        {
            var query = configurationHelper.RegistryManager.CreateQuery("SELECT * FROM devices", 100);
            while (query.HasMoreResults)
            {
                var page = await query.GetNextAsJsonAsync(); //  .GetNextAsTwinAsync();
                foreach (var json in page)
                {
                    Console.WriteLine(json);
                }
            }
        }
    }
}
