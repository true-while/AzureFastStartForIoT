using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Diagnostics;

static class AzureIoTHub
{
    //
    // Note: this connection string is specific to the device "device1". To configure other devices,
    // see information on iothub-explorer at http://aka.ms/iothubgetstartedVSCS
    //
    const string deviceConnectionString = "HostName=devhubb99.azure-devices.net;DeviceId=device1;SharedAccessKey=UShGPPPi5Td1vCzYGEvE0/ZiYUDlb2YwsmHOEG91pIs=";

    //
    // To monitor messages sent to device "device1" use iothub-explorer as follows:
    //    iothub-explorer HostName=devhubb99.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=uZKiXmf46ZD79Jr/Vv1qCX4o+nkceLFnKr/L84uxN+Y= monitor-events "device1"
    //

    // Refer to http://aka.ms/azure-iot-hub-vs-cs-wiki for more information on Connected Service for Azure IoT Hub

    public static async Task SendDeviceToCloudMessageAsync(string uid)
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);

        var telemetryDataPoint = new
        {
            deviceId = "device1",
            Time = DateTime.UtcNow,
            RFiD = uid,
            Location = "StoreLocation123"
        };

        var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
        var message = new Message(Encoding.ASCII.GetBytes(messageString));

        Debug.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
        await deviceClient.SendEventAsync(message);
        Debug.WriteLine("Done.");

        //Task.Delay(10000).Wait();
    }

    public static async Task<string> ReceiveCloudToDeviceMessageAsync()
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);

        while (true)
        {
            var receivedMessage = await deviceClient.ReceiveAsync();

            if (receivedMessage != null)
            {
                var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                await deviceClient.CompleteAsync(receivedMessage);
                return messageData;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
