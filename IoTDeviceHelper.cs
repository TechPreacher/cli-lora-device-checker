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
                isValid = ValidateAbpDevice(
                    new Program.AddAbpOptions()
                    {
                        AppEui = appEUI, 
                        AppKey = appKey,
                        SensorDecoder = sensorDecoder,
                        ClassType = classType
                    });
            }

            else if (isOTAA && !isABP)
            {
                Console.WriteLine("OTAA device configuration detected.");
                isValid = ValidateOtaaDevice(
                    new Program.AddOtaaOptions()
                    {
                        NwkSKey = nwkSKey,
                        AppSKey = appSKey,
                        DevAddr = devAddr,
                        SensorDecoder = sensorDecoder,
                        ClassType = classType
                    });
            }

            else
            {
                Console.WriteLine("Error in configuration: Can't determine if OTAA or ABP device.");
                isValid = false;
            }

            if (isValid)
            {
                Console.Write($"The configuration for device {devEui} ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("is valid.");
                Console.ResetColor();
            }

            else
            {
                Console.Write($"Error: The configuration for device {devEui} ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("is NOT valid.");
                Console.ResetColor();
            }

            return isValid;
        }

        public Program.AddAbpOptions CompleteMissingAbpOptionsAndClean(Program.AddAbpOptions opts)
        {
            if (string.IsNullOrEmpty(opts.DevEui))
            {
                opts.DevEui = Keygen.Generate(16);
                Console.WriteLine($"Info: Generating missing DevEUI: {opts.DevEui}");
            }
            else
            {
                opts.DevEui = ValidationHelper.CleanString(opts.DevEui);
            }

            if (string.IsNullOrEmpty(opts.AppEui))
            {
                opts.AppEui = Keygen.Generate(16);
                Console.WriteLine($"Info: Generating missing AppEUI: {opts.AppEui}");
            }
            else
            {
                opts.AppEui = ValidationHelper.CleanString(opts.AppEui);
            }

            if (string.IsNullOrEmpty(opts.AppKey))
            {
                opts.AppKey = Keygen.Generate(16);
                Console.WriteLine($"Info: Generating missing AppKey: {opts.AppKey}");
            }
            else
            {
                opts.AppKey = ValidationHelper.CleanString(opts.AppKey);
            }

            if (!string.IsNullOrEmpty(opts.GatewayId))
            {
                opts.GatewayId = ValidationHelper.CleanString(opts.GatewayId);
            }

            if (!string.IsNullOrEmpty(opts.SensorDecoder))
            {
                opts.SensorDecoder = ValidationHelper.CleanString(opts.SensorDecoder);
            }

            if (!string.IsNullOrEmpty(opts.ClassType))
            {
                opts.ClassType = ValidationHelper.CleanString(opts.ClassType);
            }

            return opts;
        }

        public Program.AddOtaaOptions CompleteMissingOtaaOptionsAndClean(Program.AddOtaaOptions opts)
        {
            if (string.IsNullOrEmpty(opts.DevEui))
            {
                opts.DevEui = Keygen.Generate(16);
                Console.WriteLine($"Info: Generating missing DevEUI: {opts.DevEui}");
            }
            else
            {
                opts.DevEui = ValidationHelper.CleanString(opts.DevEui);
            }

            if (string.IsNullOrEmpty(opts.NwkSKey))
            { 
                opts.NwkSKey = Keygen.Generate(16);
                Console.WriteLine($"Info: Generating missing NwkSKey: {opts.NwkSKey}");
            }
            {
                opts.NwkSKey = ValidationHelper.CleanString(opts.NwkSKey);
            }

            if (string.IsNullOrEmpty(opts.AppSKey))
            { 
                opts.AppSKey = Keygen.Generate(16);
                Console.WriteLine($"Info: Generating missing AppSKey: {opts.AppSKey}");
            }
            {
                opts.AppSKey = ValidationHelper.CleanString(opts.AppSKey);
            }

            if (string.IsNullOrEmpty(opts.DevAddr))
            { 
                opts.DevAddr = Keygen.Generate(4);
                Console.WriteLine($"Info: Generating missing DevAddr: {opts.DevAddr}");
            }
            {
                opts.DevAddr = ValidationHelper.CleanString(opts.DevAddr);
            }

            if (!string.IsNullOrEmpty(opts.GatewayId))
            {
                opts.GatewayId = ValidationHelper.CleanString(opts.GatewayId);
            }

            if (!string.IsNullOrEmpty(opts.SensorDecoder))
            {
                opts.SensorDecoder = ValidationHelper.CleanString(opts.SensorDecoder);
            }

            if (!string.IsNullOrEmpty(opts.ClassType))
            {
                opts.ClassType = ValidationHelper.CleanString(opts.ClassType);
            }

            return opts;
        }

        public bool ValidateAbpDevice(Program.AddAbpOptions opts)
        {
            var validationError = string.Empty;
            bool isValid = true;

            if (string.IsNullOrEmpty(opts.AppEui))
            {
                Console.WriteLine("Error: AppEUI is missing.");
                isValid = false;
            }
            if (!ValidationHelper.ValidateKey(opts.AppEui, 16, out validationError))
            {
                Console.WriteLine($"Error: AppEUI is invalid. {validationError}");
                isValid = false;
            }
            else
            {
                Console.WriteLine($"Info: AppEui is valid: {opts.AppEui}");
            }

            if (string.IsNullOrEmpty(opts.AppKey))
            {
                Console.WriteLine("Error: AppKey is missing");
                isValid = false;
            }
            if (!ValidationHelper.ValidateKey(opts.AppKey, 16, out validationError))
            {
                Console.WriteLine($"Error: AppKey is invalid. {validationError}");
                isValid = false;
            }
            else
            {
                Console.WriteLine($"Info: AppKey is valid: {opts.AppKey}");
            }

            if (!ValidationHelper.ValidateSensorDecoder(opts.SensorDecoder, out validationError))
            {
                isValid = false;
            }
            if (!string.IsNullOrEmpty(validationError))
            {
                Console.WriteLine(validationError);
            }
            else
            {
                Console.WriteLine($"Info: SensorDecoder is valid: {opts.SensorDecoder}");
            }

            if (!string.IsNullOrEmpty(opts.ClassType))
            {
                if (!string.Equals(opts.ClassType, "C", StringComparison.CurrentCultureIgnoreCase))
                {
                    isValid = false;
                    Console.WriteLine("Error: If ClassType is set, it needs to be \"C\".");
                }
                else
                {
                    Console.WriteLine($"Info: ClassType is valid: {opts.ClassType}");
                }
            }
            else
            {
                Console.WriteLine($"Info: ClassType is empty.");
            }

            return isValid;
        }

        public bool ValidateOtaaDevice(Program.AddOtaaOptions opts)
        {
            var validationError = string.Empty;
            bool isValid = true;
            
            if (string.IsNullOrEmpty(opts.NwkSKey))
            {
                Console.WriteLine("Error: NwkSKey is missing.");
                isValid = false;
            }
            if (!ValidationHelper.ValidateKey(opts.NwkSKey, 16, out validationError))
            {
                Console.WriteLine($"Error: NwkSKey is invalid. {validationError}");
                isValid = false;
            }
            else
            {
                Console.WriteLine($"Info: NwkSKey is valid: {opts.NwkSKey}");
            }

            if (string.IsNullOrEmpty(opts.AppSKey))
            {
                Console.WriteLine("Error: AppSKey is missing.");
                isValid = false;
            }
            if (!ValidationHelper.ValidateKey(opts.AppSKey, 16, out validationError))
            {
                Console.WriteLine($"Error: AppSKey is invalid. {validationError}");
                isValid = false;
            }
            else
            {
                Console.WriteLine($"Info: AppSKey is valid: {opts.AppSKey}");
            }

            if (string.IsNullOrEmpty(opts.DevAddr))
            {
                Console.WriteLine("Error: DevAddr is missing.");
                isValid = false;
            }
            if (!ValidationHelper.ValidateKey(opts.DevAddr, 4, out validationError))
            {
                Console.WriteLine($"Error: DevAddr is invalid. {validationError}");
                isValid = false;
            }
            else
            {
                Console.WriteLine($"Info: DevAddr is valid: {opts.DevAddr}");
            }

            if (!ValidationHelper.ValidateSensorDecoder(opts.SensorDecoder, out validationError))
            {
                isValid = false;
            }
            if (!string.IsNullOrEmpty(validationError))
            {
                Console.WriteLine(validationError);
            }
            else
            {
                Console.WriteLine($"Info: SensorDecoder is valid: {opts.SensorDecoder}");
            }

            if (!string.IsNullOrEmpty(opts.ClassType))
            {
                if (!string.Equals(opts.ClassType, "C", StringComparison.CurrentCultureIgnoreCase))
                {
                    isValid = false;
                    Console.WriteLine("Error: If ClassType is set, it needs to be \"C\".");
                }
                else
                {
                    Console.WriteLine($"Info: ClassType is valid: {opts.ClassType}");
                }
            }
            else
            {
                Console.WriteLine($"Info: ClassType is empty");
            }

            return isValid;
        }



        public async Task<bool> AddAbpDevice(Program.AddAbpOptions opts, ConfigurationHelper configurationHelper)
        {
            Console.WriteLine($"Adding ABP device to IoT Hub: {opts.DevEui} ...");

            bool isSuccess = false;
            var twinProperties = new TwinProperties();

            twinProperties.Desired["AppEUI"] = opts.AppEui;

            twinProperties.Desired["AppKey"] = opts.AppKey;

            twinProperties.Desired["GatewayID"] = opts.GatewayId;

            twinProperties.Desired["SensorDecoder"] = opts.SensorDecoder;

            if (!string.IsNullOrEmpty(opts.ClassType))
            {
                twinProperties.Desired["ClassType"] = opts.ClassType;
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
                Console.WriteLine($"Error: {ex.Message}\n");
                return false;
            }

            if (result.IsSuccessful)
            {
                Console.WriteLine($"Success!\n");
                isSuccess = true;
            }
            else
            {
                Console.WriteLine($"Error adding device:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Device Id: {error.DeviceId}, Code: {error.ErrorCode}, Error: {error.ErrorStatus}");
                }
                Console.WriteLine();
                isSuccess = false;
            }
            return isSuccess;
        }

        public async Task<bool> AddOtaaDevice(Program.AddOtaaOptions opts, ConfigurationHelper configurationHelper)
        {
            Console.WriteLine($"Adding OTAA device to IoT Hub: {opts.DevEui} ...");

            bool isSuccess = false;
            var twinProperties = new TwinProperties();

            twinProperties.Desired["AppSKey"] = opts.AppSKey;

            twinProperties.Desired["NwkSKey"] = opts.NwkSKey;

            twinProperties.Desired["DevAddr"] = opts.DevAddr;

            twinProperties.Desired["GatewayID"] = opts.GatewayId;

            twinProperties.Desired["SensorDecoder"] = opts.SensorDecoder;

            if (!string.IsNullOrEmpty(opts.ClassType))
            {
                twinProperties.Desired["ClassType"] = opts.ClassType;
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
                Console.WriteLine($"Error: {ex.Message}\n");
                return false;
            }

            if (result.IsSuccessful)
            {
                Console.WriteLine($"Done!\n");
                isSuccess = true;
            }
            else
            {
                Console.WriteLine($"Error adding device:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Device Id: {error.DeviceId}, Code: {error.ErrorCode}, Error: {error.ErrorStatus}");
                }
                Console.WriteLine();
                isSuccess = false;
            }
            return isSuccess;
        }

        public async Task QueryDevices(ConfigurationHelper configurationHelper)
        {
            var query = configurationHelper.RegistryManager.CreateQuery("SELECT * FROM devices", 100);
            while (query.HasMoreResults)
            {
                var page = await query.GetNextAsJsonAsync(); // .GetNextAsTwinAsync();
                foreach (var json in page)
                {
                    Console.WriteLine(json);
                }
            }
        }
    }
}
