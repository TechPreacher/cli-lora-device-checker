using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace cli_lora_device_checker
{
    public class ConfigurationHelper
    {
        public string ConnectionString { get; set; }
        public RegistryManager RegistryManager { get; set; }

        public bool ReadConfig()
        {
            var connectionString = string.Empty;

            try
            {
                // Get configuration
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("settings.json", optional: false, reloadOnChange: true)
                    .Build();

                connectionString = config["IoTHubConnectionString"];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("The format should be: { \"IoTHubConnectionString\" : \"HostName=xxx.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=xxx\" }");
                return false;
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Using connection string: {connectionString} \n");
            Console.ResetColor();

            try
            {
                RegistryManager = RegistryManager.CreateFromConnectionString(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to IoT Hub (possible error in connection string): {ex.Message}");
                return false;
            }

            return true;
        }
    }
}
