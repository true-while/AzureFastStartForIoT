using RFIDReader.Mfrc522Lib;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace RFIDReader
{
    public sealed class StartupTask : IBackgroundTask
    {   
        Mfrc522 mfrc;
        static GpioPin pin;
        const int LED_PIN = 5;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Running....");
            InitGPIO();
            pin.Write(GpioPinValue.High);

            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
          
            mfrc = new Mfrc522();
            await mfrc.InitIO();

            // Main loop
            while (true)
            {          
                //true when an RFiD tag is detected
                if (mfrc.IsTagPresent())
                {
                    pin.Write(GpioPinValue.Low);
                    Debug.WriteLine("IsTagPresent() == True.");
                    //read the UUID from the card
                    Uid uid = mfrc.ReadUid();
                    Debug.WriteLine("Tag value == " + uid.ToString());
                    //rfid controller halt state
                    //mfrc.HaltTag();

                    //Send message to IoT Hub
                    await AzureIoTHub.SendDeviceToCloudMessageAsync(uid.ToString());
                    pin.Write(GpioPinValue.High);
                }
                                   
            //sleep
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(1));

            }
            //deferral.Complete();
        }

        private void InitGPIO()
        {
            GpioController gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                pin = null;
                return;
            }

            pin = gpio.OpenPin(LED_PIN);

            if (pin == null)
            {
                return;
            }

            pin.Write(GpioPinValue.High);
            pin.SetDriveMode(GpioPinDriveMode.Output);
        }

    }
}
