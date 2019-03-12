namespace cli_lora_device_checker
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using CommandLine;
    using Newtonsoft.Json.Linq;
    using System.Linq;

    public class Program
    {
        static ConfigurationHelper configurationHelper = new ConfigurationHelper();
        static IoTDeviceHelper iotDeviceHelper = new IoTDeviceHelper();

        [Verb("verify", HelpText = "Verify device.")]
        public class VerifyOptions
        {
            [Option("deveui",
                Required = true,
                HelpText = "DevEUI / Device Id.")]
            public string DevEui { get; set; }
        }

        [Verb("show", HelpText = "Show device twin.")]
        public class ShowOptions
        {
            [Option("deveui",
                Required = true,
                HelpText = "DevEUI / Device Id.")]
            public string DevEui { get; set; }
        }

        [Verb("addabpdevice", HelpText = "Add new ABP device.")]
        public class AddAbpOptions {

            [Option("deveui",
                Required = true,
                HelpText = "DevEUI / Device Id.")]
            public string DevEui { get; set; }

            [Option("appeui",
                Required = true,
                HelpText = "AppEUI.")]
            public string AppEui { get; set; }

            [Option("appkey",
                Required = true,
                HelpText = "AppKey.")]
            public string AppKey { get; set; }

            [Option("gatewayid",
                Required = false,
                HelpText = "GatewayID (optional).")]
            public string GatewayId { get; set; }

            [Option("decoder",
                Required = false,
                HelpText = "SensorDecoder (optional).")]
            public string SensorDecoder { get; set; }

            [Option("classtype",
                Required = false,
                HelpText = "ClassType (optional).")]
            public string ClassType { get; set; }
        }

        [Verb("addotaadevice", HelpText = "Add new OTAA device.")]
        public class AddOtaaOptions
        {

            [Option("deveui",
                Required = true,
                HelpText = "DevEUI / Device Id.")]
            public string DevEui { get; set; }

            [Option("appskey",
                Required = true,
                HelpText = "AppSKey.")]
            public string AppSKey { get; set; }

            [Option("nwkskey",
                Required = true,
                HelpText = "NwkSKey.")]
            public string NwkSKey { get; set; }

            [Option("devaddr",
                Required = true,
                HelpText = "DevAddr.")]
            public string DevAddr { get; set; }

            [Option("gatewayid",
                Required = false,
                HelpText = "GatewayID (optional).")]
            public string GatewayId { get; set; }

            [Option("decoder",
                Required = false,
                HelpText = "SensorDecoder (optional).")]
            public string SensorDecoder { get; set; }

            [Option("classtype",
                Required = false,
                HelpText = "SensorDecoder (optional).")]
            public string ClassType { get; set; }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Azure IoT Edge LoRaWAN Starter Kit LoRa Leaf Device Verification Utility. http://aka.ms/lora");

            var success = Parser.Default.ParseArguments< ShowOptions, VerifyOptions, AddAbpOptions, AddOtaaOptions>(args)
                .MapResult(
                    (ShowOptions opts) => RunShowAndReturnExitCode(opts),
                    (VerifyOptions opts) => RunVerifyAndReturnExitCode(opts),
                    (AddAbpOptions opts) => RunAddAbpAndReturnExitCode(opts),
                    (AddOtaaOptions opts) => RunAddOtaaAndReturnExitCode(opts),
                    errs => false
                );

            if ((bool)success)
                Console.WriteLine("Successfully terminated.");
            else
                Console.WriteLine("Terminated with errors.");
        }

        private static object RunShowAndReturnExitCode(ShowOptions opts)
        {
            if (!configurationHelper.ReadConfig())
                return false;

            Console.WriteLine($"Querying DevEUI: {opts.DevEui} ...");
            var twinData = iotDeviceHelper.QueryTwinSingle(opts.DevEui, configurationHelper).Result;
            if (twinData != null)
            {
                Console.WriteLine(twinData.ToString());
                return true;
            }
            else
            {
                Console.WriteLine($"Could not get data for device {opts.DevEui}.");
                return false;
            }
        }

        private static object RunVerifyAndReturnExitCode(VerifyOptions opts)
        {
            if (!configurationHelper.ReadConfig())
                return false;

            Console.WriteLine($"Querying DevEUI: {opts.DevEui} ...");
            var twinData = iotDeviceHelper.QueryTwinSingle(opts.DevEui, configurationHelper).Result;

            if (twinData != null)
            {
                Console.WriteLine(twinData.ToString());

                Console.WriteLine($"Verify Device: {opts.DevEui} ...");
                return iotDeviceHelper.VerifyTwinSingle(opts.DevEui, twinData);
            }
            else
            {
                Console.WriteLine($"Could not get data for device {opts.DevEui}.");
                return false;
            }
        }

        private static object RunAddAbpAndReturnExitCode(AddAbpOptions opts)
        {
            if (!configurationHelper.ReadConfig())
                return false;

            bool isSuccess = false;

            if (iotDeviceHelper.ValidateAbpDevice(opts.AppEui, opts.AppKey, opts.SensorDecoder, opts.ClassType, out string error))
            {
                isSuccess = iotDeviceHelper.AddAbpDevice(opts, configurationHelper).Result;
            }
            else
            {
                Console.WriteLine("Can not add ABP device. The following errors occurred: \n" + error);
            }

            if (isSuccess)
            { 
                Console.WriteLine($"Querying DevEUI: {opts.DevEui} ...");
                var twinData = iotDeviceHelper.QueryTwinSingle(opts.DevEui, configurationHelper).Result;
                Console.WriteLine(twinData.ToString());
            }

            return isSuccess;
        }

        private static object RunAddOtaaAndReturnExitCode(AddOtaaOptions opts)
        {
            if (!configurationHelper.ReadConfig())
                return false;

            bool isSuccess = false;

            if (iotDeviceHelper.ValidateSensorDecoder(opts.SensorDecoder))
            {
                isSuccess = iotDeviceHelper.AddOtaaDevice(opts, configurationHelper).Result;
            }

            if (isSuccess)
            {
                Console.WriteLine($"Querying DevEUI: {opts.DevEui} ...");
                var twinData = iotDeviceHelper.QueryTwinSingle(opts.DevEui, configurationHelper).Result;
                Console.WriteLine(twinData.ToString());
            }

            return isSuccess;
        }
    }
}
