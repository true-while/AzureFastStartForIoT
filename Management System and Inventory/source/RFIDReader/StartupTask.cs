using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;

using RFIDReader.Mfrc522Lib;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Client;


// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace RFIDReader
{
    public sealed class StartupTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            Mfrc522 mfrc = new Mfrc522();
            await mfrc.InitIO();

            #region loop
            while (true)
            {
                

                //verify if the tag is present
                if (mfrc.IsTagPresent())
                {
                    Debug.WriteLine("IsTagPresent() == True.");
                    //read the UUID and show in UI
                    var uid = mfrc.ReadUid();
                    Debug.WriteLine("Tag value == " + uid.ToString());
                    //rfid controller halt state
                    //mfrc.HaltTag();
                    //Sent message
                    await AzureIoTHub.SendDeviceToCloudMessageAsync(uid.ToString());
                }
                                   
            //sleep
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(1));

            }
            #endregion

            //deferral.Complete();
        }
    }
}
