namespace cli_lora_device_checker
{
    using System;
    using CommandLine;

    public class Program
    {
        static ConfigurationHelper configurationHelper = new ConfigurationHelper();
        static IoTDeviceHelper iotDeviceHelper = new IoTDeviceHelper();

        [Verb("list", HelpText = "Lits devices.")]
        public class ListOptions
        {
            [Option("page",
                Required = false,
                HelpText = "Devices per page. Default is 10.")]
            public string Page { get; set; }

            [Option("total",
                Required = false,
                HelpText = "Maximum number of devices to list. Default is all.")]
            public string Total { get; set; }
        }

        [Verb("verify", HelpText = "Verify a single device.")]
        public class VerifyOptions
        {
            [Option("deveui",
                Required = true,
                HelpText = "DevEUI / Device Id.")]
            public string DevEui { get; set; }
        }

        [Verb("query", HelpText = "Query a device twin.")]
        public class QueryOptions
        {
            [Option("deveui",
                Required = true,
                HelpText = "DevEUI / Device Id.")]
            public string DevEui { get; set; }
        }

        [Verb("addabpdevice", HelpText = "Add a new ABP device.")]
        public class AddAbpOptions {

            [Option("deveui",
                Required = false,
                HelpText = "DevEUI / Device Id. Will be randomly generated if left blank.")]
            public string DevEui { get; set; }

            [Option("appeui",
                Required = false,
                HelpText = "AppEUI. Will be randomly generated if left blank.")]
            public string AppEui { get; set; }

            [Option("appkey",
                Required = false,
                HelpText = "AppKey. Will be randomly generated if left blank.")]
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

        [Verb("addotaadevice", HelpText = "Add a new OTAA device.")]
        public class AddOtaaOptions
        {

            [Option("deveui",
                Required = false,
                HelpText = "DevEUI / Device Id. Will be randomly generated if left blank.")]
            public string DevEui { get; set; }

            [Option("appskey",
                Required = false,
                HelpText = "AppSKey. Will be randomly generated if left blank.")]
            public string AppSKey { get; set; }

            [Option("nwkskey",
                Required = false,
                HelpText = "NwkSKey. Will be randomly generated if left blank.")]
            public string NwkSKey { get; set; }

            [Option("devaddr",
                Required = false,
                HelpText = "DevAddr. Will be randomly generated if left blank.")]
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
            Console.WriteLine("Azure IoT Edge LoRaWAN Starter Kit LoRa Leaf Device Verification Utility.");
            Console.Write("This tool complements ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("http://aka.ms/lora \n");
            Console.ResetColor();

            var success = Parser.Default.ParseArguments<ListOptions, QueryOptions, VerifyOptions, AddAbpOptions, AddOtaaOptions>(args)
                .MapResult(
                    (ListOptions opts) => RunListAndReturnExitCode(opts),
                    (QueryOptions opts) => RunQueryAndReturnExitCode(opts),
                    (VerifyOptions opts) => RunVerifyAndReturnExitCode(opts),
                    (AddAbpOptions opts) => RunAddAbpAndReturnExitCode(opts),
                    (AddOtaaOptions opts) => RunAddOtaaAndReturnExitCode(opts),
                    errs => false
                );

            if ((bool)success)
            { 
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nSuccessfully terminated.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nTerminated with errors.");
                Console.ResetColor();
            }
        }

        private static object RunListAndReturnExitCode(ListOptions opts)
        {
            int page;
            int total;

            if (!configurationHelper.ReadConfig())
                return false;

            if (!int.TryParse(opts.Page, out page))
                page = 10;

            if (!int.TryParse(opts.Total, out total))
                total = -1;

            Console.WriteLine($"Listing devices...");
            Console.WriteLine($"Page: {page}, Total: {total}\n");

            var isSuccess = iotDeviceHelper.QueryDevices(configurationHelper, page, total).Result;

            Console.WriteLine();
            Console.WriteLine("Done.");
            return isSuccess;

        }

        private static object RunQueryAndReturnExitCode(QueryOptions opts)
        {
            if (!configurationHelper.ReadConfig())
                return false;

            Console.WriteLine($"Querying DevEUI: {opts.DevEui} ...\n");

            var twinData = iotDeviceHelper.QueryTwinSingle(opts.DevEui, configurationHelper).Result;
            if (twinData != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(twinData.ToString());
                Console.ResetColor();
                Console.WriteLine();
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

            Console.WriteLine($"Querying DevEUI: {opts.DevEui} ...\n");

            var twinData = iotDeviceHelper.QueryTwinSingle(opts.DevEui, configurationHelper).Result;

            if (twinData != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(twinData.ToString());
                Console.ResetColor();
                Console.WriteLine();

                Console.WriteLine($"Verifying Device: {opts.DevEui} ...\n");
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

            opts = iotDeviceHelper.CompleteMissingAbpOptionsAndClean(opts);

            if (iotDeviceHelper.ValidateAbpDevice(opts))
            {
                isSuccess = iotDeviceHelper.AddAbpDevice(opts, configurationHelper).Result;
            }
            else
            {
                Console.WriteLine("Can not add ABP device.");
            }

            if (isSuccess)
            { 
                Console.WriteLine($"Querying DevEUI: {opts.DevEui} ...\n");
                Console.ForegroundColor = ConsoleColor.Yellow;
                var twinData = iotDeviceHelper.QueryTwinSingle(opts.DevEui, configurationHelper).Result;
                Console.WriteLine(twinData.ToString());
                Console.ResetColor();
                Console.WriteLine();
            }

            return isSuccess;
        }

        private static object RunAddOtaaAndReturnExitCode(AddOtaaOptions opts)
        {
            if (!configurationHelper.ReadConfig())
                return false;

            bool isSuccess = false;

            opts = iotDeviceHelper.CompleteMissingOtaaOptionsAndClean(opts);

            if (iotDeviceHelper.ValidateOtaaDevice(opts))
            {
                isSuccess = iotDeviceHelper.AddOtaaDevice(opts, configurationHelper).Result;
            }
            else
            {
                Console.WriteLine("Can not add OTAA device.");
            }

            if (isSuccess)
            {
                Console.WriteLine($"Querying DevEUI: {opts.DevEui} ...\n");
                Console.ForegroundColor = ConsoleColor.Yellow;
                var twinData = iotDeviceHelper.QueryTwinSingle(opts.DevEui, configurationHelper).Result;
                Console.WriteLine(twinData.ToString());
                Console.ResetColor();
                Console.WriteLine();
            }

            return isSuccess;
        }
    }
}
