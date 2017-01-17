using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Diagnostics;
using Windows.Devices.Gpio;

static class AzureIoTHub
{
    const string deviceConnectionString = "HostName=devhubb99.azure-devices.net;DeviceId=device1;SharedAccessKey=UShGPPPi5Td1vCzYGEvE0/ZiYUDlb2YwsmHOEG91pIs=";

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

        string messageString = JsonConvert.SerializeObject(telemetryDataPoint);
        Message message = new Message(Encoding.ASCII.GetBytes(messageString));

        Debug.WriteLine("{0} > Sending message: {1}", telemetryDataPoint.Time.ToString(), messageString);
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
