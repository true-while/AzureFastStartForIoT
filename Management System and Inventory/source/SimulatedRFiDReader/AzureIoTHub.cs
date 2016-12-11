using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using System.Configuration;
using Newtonsoft.Json;

static class AzureIoTHub
{
    public static async Task SendDeviceToCloudInteractiveMessagesAsync()
    {
        string deviceConnectionString = ConfigurationManager.AppSettings["DeviceKey"];
        var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);

        while (true)
        {
            var telemetryDataPoint = new
            {
                DeviceId = "device1",
                Time = DateTime.UtcNow,
                RFiD = GetRandomRFiD(),
                Location = "StoreLocation123"
            };

            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);
            Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

            Task.Delay(10000).Wait();
        }

    }

    public static async Task<string> ReceiveCloudToDeviceMessageAsync()
    {
        string deviceConnectionString = ConfigurationManager.AppSettings["DeviceKey"];

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

    private static string GetRandomRFiD()
    {
        Random rnd1 = new Random();
        string[] ids = new string[6] { "ABC123", "DEF456", "GHI789", "JKL123", "MNO456", "PQR789" };
        return ids[rnd1.Next(0, ids.Length)];
    }
}
