# cli-lora-device-checker

Command Line Interface Tool to verify LoRaWAN leaf devices configured in Azure IoT Hub for the Azure IoT Edge LoRaWAN Gateway: <http://aka.ms/lora>

## Building

You can create an platform specific executable by running

```bash
dotnet publish -c Release -r win10-x64
```

## Running

You can run the tool from the command line using using .NET Core by executing

```bash
dotnet run -- (add verbs here)
```

## Setting up

[settings.json](/settings.json) needs to be in the same directory as the cli-lora-device-checker binary (verifyloradevice.dll or verifyloradevice.exe).

[settings.json](/settings.json) needs to contain a connection string from the Azure IoT Hub you want to work with. This connection string needs to belong to a shared access policy with **registry read**, **registry write** and **service connect** permissions enabled. You can use the default policy named **iothubowner**.

```json
{
  "IoTHubConnectionString": "HostName=myiothub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=AeVMLayazGTS9QRMJtFGSSNwdhUdYR5VwCjaafc3DL0=",
}
```

## Supported commands

The following verbs are supported:

|verb|description|
|-|-|
|list|Lists devices.|
|query|Query device twin.|
|verify|Verify device.|
|addabpdevice|Add new ABP device.|
|addotaadevice|Add new OTAA device.|
|help|Display more information on a specific command.|
|version|Display version information.|

### list

The list verb supports the following parameters:

|parameter|description|
|-|-|
|--page|Devices per page. Default is 10.|
|--total|Maximum number of devices to list. Default is all.|
|--help|Display this help screen.|
|--version|Display version information.|

## query

The qurey verb supports the following parameters:

|parameter|description|
|-|-|
|--deveui|Required. DevEUI / Device Id.|
|--help|Display this help screen.|
|--version|Display version information.|

## verify

The qurey verb supports the following parameters:

|parameter|description|
|-|-|
|--deveui|Required. DevEUI / Device Id.|
|--help|Display this help screen.|
|--version|Display version information.|

## addabpdevice

|parameter|description|
|-|-|
|--deveui|DevEUI / Device Id. Will be randomly generated if left blank.|
|--appeui|AppEUI. Will be randomly generated if left blank.|
|--appkey|AppKey. Will be randomly generated if left blank.|
|--gatewayid|GatewayID (optional).|
|--decoder|SensorDecoder (optional).|
|--classtype|ClassType (optional).|
|--help|Display this help screen.|
|--version|Display version information.|

## addotaadevice

|parameter|description|
|-|-|
|--deveui|DevEUI / Device Id. Will be randomly generated if left blank.|
|--appskey|AppSKey. Will be randomly generated if left blank.|
|--nwkskey|NwkSKey. Will be randomly generated if left blank.|
|--devaddr|DevAddr. Will be randomly generated if left blank.|
|--gatewayid|GatewayID (optional).|
|--decoder|SensorDecoder (optional).|
|--classtype|SensorDecoder (optional).|
|--help|Display this help screen.|
|--version|Display version information.|
