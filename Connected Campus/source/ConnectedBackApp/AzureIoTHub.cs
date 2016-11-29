using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using System.Diagnostics;

static class AzureIoTHub
{
    //
    // Note: this connection string is specific to the device "device1". To configure other devices,
    // see information on iothub-explorer at http://aka.ms/iothubgetstartedVSCS
    //
    const string deviceConnectionString = "HostName=deviothub99.azure-devices.net;DeviceId=device1;SharedAccessKey=dLejUkq7DIePTnS7r+9//GiyuJwvJz11fo2adEkRnU8=";

    //
    // To monitor messages sent to device "device1" use iothub-explorer as follows:
    //    iothub-explorer HostName=deviothub99.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=1upu+fk+xn7ODSJFOtT5ObDdILkaNJOm9MtROPHAJTo= monitor-events "device1"
    //

    // Refer to http://aka.ms/azure-iot-hub-vs-cs-wiki for more information on Connected Service for Azure IoT Hub

    public static async Task SendDeviceToCloudMessageAsync(string data)
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Amqp);
        var message = new Message(Encoding.ASCII.GetBytes(data));
        await deviceClient.SendEventAsync(message);
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
                //return messageData;
                Debug.WriteLine(messageData);
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
   
}
