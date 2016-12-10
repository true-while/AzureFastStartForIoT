using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using System.Diagnostics;
using System.Threading;


// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace RFiDReader
{
    public sealed class StartupTask : IBackgroundTask
    {
        private I2cDevice I2CAccel;
        int I2C_ADDR = 0x48;
        Timer periodicTimer;

        byte[] buffer;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            buffer = new byte[1024];

            InitI2CAccel();

            periodicTimer = new Timer(this.TimerCallback, null, 0, 1000);

            //deferral.Complete();
        }

        private void TimerCallback(object state)
        {
            try
            {
                I2CAccel.Read(buffer);
                Debug.Write(buffer);
            }
            catch (Exception)
            {
                Debug.Write("Read error!");
            }
            
        }

        private async void InitI2CAccel()
        {
            string error=String.Empty; 

            try
            {
                var settings = new I2cConnectionSettings(I2C_ADDR);
                settings.BusSpeed = I2cBusSpeed.FastMode;                       /* 400KHz bus speed */

                string aqs = I2cDevice.GetDeviceSelector();                     /* Get a selector string that will return all I2C controllers on the system */
                var dis = await DeviceInformation.FindAllAsync(aqs);            /* Find the I2C bus controller devices with our selector string             */
                I2CAccel = await I2cDevice.FromIdAsync(dis[0].Id, settings);    /* Create an I2cDevice with our selected bus controller and I2C settings    */
                if (I2CAccel == null)
                {
                    error = string.Format(
                        "Slave address {0} on I2C Controller {1} is currently in use by " +
                        "another application. Please ensure that no other applications are using I2C.",
                        settings.SlaveAddress,
                        dis[0].Id);
                    Debug.Write(error);
                    return;
                }

            }
            catch (Exception ex)
            {
                error = "Failed to communicate with device: " + ex.Message;
                Debug.Write(error);
                return;
            }

        }
    }
}
