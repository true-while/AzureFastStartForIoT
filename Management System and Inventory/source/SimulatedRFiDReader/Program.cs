using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulatedRFiDReader
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () => {

                await AzureIoTHub.SendDeviceToCloudInteractiveMessagesAsync();

            }).Wait();
        }
    }
}
