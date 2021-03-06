# Connected Campus

![](images/meetingroom.png)

Scenario
========

In this scenario you will build an application for a Windows 10 Core device with several attached sensors.
Every 10 seconds it will send information about the environmental conditions of a room in which it is located to Microsoft Azure.
This data will be displayed on a dashboard allowing a user to see whether a room is free, if the lights are on and what the temperature is.

Credits: Full credits for this scenario go to:

* [Intelligent Multi-Conference Room using UWP App and Azure](https://microsoft.hackster.io/en-US/Kishore10211/intelligent-multi-conference-room-using-uwp-app-and-azure-620b1d) - Made by [Kishore Gaddam](https://microsoft.hackster.io/en-US/Kishore10211).

Additional content has been added by

* [Microsoft Premier Services](https://www.microsoft.com/en-us/microsoftservices/support.aspx).

Architecture
============

The system you are about to build consists of the following components and will work in the following manner:

1. __An IoT Hub__. This will act as the main gateway for ingesting data from connected devices. You will need to register your device with IoT Hub before you can send it data.
2. A __Raspberry Pi__ with attached sensors. A motion sensor will be used determine wether anybody is in the room with light and temprature data being collected via appropriate sensors.
3. The collected data will be uploaded to IoTHub.
4. An application will be created to allow the viewing of the raw data once it has been uploaded.
5. The raw data will be feed into __Azure Stream Analytics__ to produce averages of temprature overtime.
6. The final data will be sent to __Power BI__ for display. This will make it easly available to view on a number of mobile devices or computers.

![](images/overview.png)

Basic Hardware Setup
====================

As described in the [Kit List for these scenarios](/Electronics/Kit%20List.md), you will need the following hardware items to build this scenario:

* [Microsoft IoT Pack for Raspberry Pi 3 - w/ Raspberry Pi 3](https://www.adafruit.com/products/2733)
* [1 x USB cable - A/MicroB - 3ft](https://www.adafruit.com/product/592)
* [1 x Adafruit Assembled Pi Cobbler Breakout + Cable for Raspberry Pi - Model B](https://www.adafruit.com/product/914)
* [1 x PIR (motion) sensor](https://www.adafruit.com/product/189)

Specifically the following items are required from the kit list:

* Raspberry Pi
* Breadboard (generic)
* PIR Motion Sensor
* Adafruit BMP280 Barometric Pressure & Altitude Sensor
* MCP3008 - 8-Channel 10-Bit ADC With SPI Interface
* 2 x Potentiometer - 10K
* Resistor 10k ohm
* Resistor 560 ohm
* An LED
* Jumper Wires

## Basic Circuit

Build your basic cicuit according to this diagram:

![The circuit](images/circuit.png)

If you have more electronics experience, this is the circuit diagram of what you'll be building:

![The circuit diagram](images/circuitdiagram.png)

For interest/reference, these are the PINs on the Raspberry Pi. Note for example that the LED is attached to GPIO Pin 5 - this is referenced later in the code.

![Raspberry Pi 3 PinOut Reference](/RaspberryPI/images/pinout.png)

## "Here's one I made earlier"

![Completed circuit example](images/prototype.png)

Azure Pre-reqs
==============

1. A working Azure subscription or trail - http://portal.azure.com
2. A working PowerBI subscription or trial - http://www.powerbi.com

Develoment Machine and IoT device Setup
========================================

1. Ensure your local development machine is setup according to these instructions: [Azure IoT Development machine setup](../IoT Developer Setup.docx?raw=true).
2. Part of the above document describes installing the "Device Explorer" tool - make sure that you *do* follow these instructions as you'll need that tool later on.
3. Ensure you have installed the [Connected Service for Azure IoT Hub Visual Studio Extension](https://marketplace.visualstudio.com/items?itemName=MicrosoftIoT.ConnectedServiceforAzureIoTHub)
4. Ensure you have [followed the instructions](https://developer.microsoft.com/en-us/windows/iot/docs/iotdashboard) to __Use the Windows 10 IoT Core Dashboard__ to setup your Raspberry Pi.

*Note about setting up Wi-Fi on the RPi:* IoT Dashboard shows all available networks that your PC has previously connected to. If you don’t see your desired Wi-Fi network on the list, ensure you’re connected to it on your PC. If you uncheck the Wi-Fi box when writing the Windows 10 IoT Core image onto your SD card, you must connect an Ethernet cable to your board after flashing. Whether using Wi-Fi or Ethernet, your device will recieve an IP via DHCP at start up.

For more in depth guidance on setting up your hardware device see [Get Started](https://developer.microsoft.com/en-us/windows/iot/GetStarted).

Step 1 - Build an IoT Hub
=========================

1. [Open the Azure Portal](https://portal.azure.com).
2. Click (+)-->Internet of Things-->IoT Hub.
    ![New IoT Hub](images/newiot.png)
3. Enter a unique name for the IoT Hub, choose a Pricing and Scale tier (note that Free has been choosen here), select or create a Resource Group and datacentre location and __Click Create__.
4. ![Choosing IoT Hub settings](images/newiothubsettings.png)
5. Once the IoTHub has been created, ensure you make a copy of the *iothubowner* Connection String - this is shown via the *Shared Access Policies-->iothubowner* blade.
    ![Iot Hub Key](images/iothubkeys.png)  
6. Finally you should also make a copy of the *Event Hub-compatible name* & *Event Hub-compatible endpoint* values. You'll need these later on when you start to read data back from IoT Hub.
    ![Event Hub Compatible Endpoint](images/eventhubendpoint.png)
7. Done.

Step 2 - Register your device with IoT Hub
==========================================

For your device to connect to IoT Hub it must have its own Device Identity (aka set of credentials). The process of obtaining these is known as Registering your Device. Currently there is no way to do this via the Azure Portal but there is a remote API available. Rather than write a custom application to connect & register you are going to use Device Explorer which is part of the IoT SDK. You can also register a device via the IoT Dashboard application or use iothub-explorer, another tool from the IoT SDK written in node.js.

1.	Open the Device Explorer (*C:\Program Files (x86)\Microsoft\DeviceExplorer\DeviceExplorer.exe*) and fill the IoT Hub Connection String field with the connection string of the IoT Hub you created in previous steps and click on __Update__.
2. ![Setting the connection string](images/deconfigure.png)
3. Go to the __Management tab__ and __Click on the Create button__. The Create Device popup will be displayed. Enter "__device1__" as the Device ID for your device  and __click on Create__. *The device name is important as other parts of the supplied sample program rely on this*.
4. ![Create device entry](images/createentry.png)
5. Once the device identity is created, it will be displayed in the grid. __Right click__ on the device entry you just created and select __Copy connection string for the selected device__. Paste this into a notepad as it will be required later one.
6. ![Copy device details](images/degrid.png)

__Note__: The device identities registration can be automated using the Azure IoT Hubs SDK. An example can be found at https://azure.microsoft.com/en-us/documentation/articles/iot-hub-csharp-csharp-getstarted/#create-a-device-identity. 

Step 3 - Create an App for your device
======================================

This application will read the sensor date from your device and upload it to IoT Hub.

A [Completed Example](source/DeviceApp) is also available. __TODO__: Add details on how to set this up and use it.

1. Open Visual Studio then go *File->Project->Visual C#->Windows->Windows IoT Core* and select the *Background Application (IoT)* template.
    ![Blank project](images/newproject.png)
2. Call your project "ConnectedBackApp" and make sure the .NET Framework version is 4.5.1 or later. Click __Create__. Accept the defaults for Universal Windows Project target versions.
3. [Follow these instructions to add a NuGet reference](/Developer Setup/NuGet Package Install.md) to the __Microsoft.Azure.Devices.Client__ package.
4. Right click on References in the Solution Explorer and choose "Add Connected Service".
    ![Add reference](images/addservicereference.png)
5. Choose __Azure IoT Hub__ and press __Configure__ then select the option to __hardcode__ shared access keys in the applications code, then press OK.
    ![Add Service](images/addservice.png) ![Hardcode connection string](images/hardcode.png)
6. The wizard will now search for IoT Hubs available in your subscription, find the one you created previously and click __Add__.
    ![Wizard Search](images/wizardsearch.png)
7. Select the device you registered earlier then click OK.
    ![Add Device](images/selectdevice.png)
    * A new file __AzureIoTHub.cs__ has been added to your Visual Studio project along with several Nuget packages which reference the Azure IoT SDK. This file contains the boiler-plate code that you can immediately invoke in your application. The AzureIoTHub class contains two methods that you can start using right away from your own classes:
    * A method to send messages - __SendDeviceToCloudMessageAsync()__
    * A method to start listening for incoming messages - __ReceiveCloudToDeviceMessageAsync()__
    * You can call these methods from elsewhere in your project.
    * The Connected Service Wizard has inserted into the new class a __deviceConnectionString__ variable that contains the access key required to connect your device to IoT Hub. Anyone who comes into the possession of this information will be able to send and receive messages on behalf of that device. It is recommended that you remove this string from the source code before committing your code into a source control. Consider storing it in a configuration file or an environment variable.
8. Replace the entire StartupTask class with the following code:-  __TODO: Describe what it does__
    ```
public sealed class StartupTask : IBackgroundTask
    {
        #region Constants and variables

        // This should really be read from a config file.
        private string deviceName = "device1";

        // These defined GPIO pins on which the movement sensor and status LED will be connected.
        private const int ledPin = 5;
        private const int pirPin = 6;
        private GpioPin led;
        private GpioPin pir;

        // These represent the two sensors, i.e. the pressure/temp (BMP280) & light detector (MCP3008).
        private BMP280 bmpsensor;
        private MCP3008 mcp3008;

        // These hold the results of all the sensor data.
        private MCP3008SensorData adcSensorData;
        private BMP280SensorData bmpSensorData;

        float currentTemperature = 0;
        float currentPressure = 0;

        // Use for configuration of the MCP3008 class voltage formula
        const float ReferenceVoltage = 5.0F; // The MCP3008 works on a 5v reference voltage.

        // Values for which channels we will be using from the ADC chip
        const byte LowPotentiometerADCChannel = 0;
        const byte HighPotentiometerADCChannel = 1;
        const byte CDSADCChannel = 2;

        // Some strings to let us know the current state.
        const string JustRightLightString = "Bright";//Ah, just right
        const string LowLightString = "Dark";//I need a light
        const string HighLightString = "Too Bright";

        // Some internal state information
        enum eState { unknown, JustRight, TooBright, TooDark };
        eState CurrentState = eState.unknown;
        #endregion

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // Using BackgroundTaskDeferral
            // as described in http://aka.ms/backgroundtaskdeferral
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            bmpsensor = new BMP280();
            await bmpsensor.Initialize();

            mcp3008 = new MCP3008(ReferenceVoltage);
            mcp3008.Initialize();

            InitGPIO();

            bmpSensorData = new BMP280SensorData();
            adcSensorData = new MCP3008SensorData();

            string roomstatus = String.Empty;

            while (true)
            {
                Debug.WriteLine("Reading taken at " + DateTime.UtcNow);

                #region Read pressure and temperature data from sensor
                bmpSensorData.Temperature = await bmpsensor.ReadTemperature();
                bmpSensorData.Pressure = await bmpsensor.ReadPreasure();           
                Debug.WriteLine(bmpSensorData.Temperature + " deg C");
                Debug.WriteLine(bmpSensorData.Pressure + " Pa");
                Debug.WriteLine(bmpSensorData.Pressure / 100000 + " Bar");
                Debug.WriteLine(bmpSensorData.Pressure / 100 + " milliBar");
                #endregion

                #region Reading Lighting Information from Sensor  
                adcSensorData = ReadLightStatusInRoom();
                Debug.WriteLine("Light Status in Room: " + adcSensorData.lightStatus);
                #endregion

                #region Read the motion sensor
                // If sensor pin is high, then motion was detected
                if (pir.Read() == GpioPinValue.High)
                {
                    // turn on the LED
                    led.Write(GpioPinValue.Low);
                    Debug.WriteLine("Motion detected - Room is occupied");
                    roomstatus = "Occupied";
                    
                }
                else
                {
                    // turn off the LED
                    led.Write(GpioPinValue.High);
                    Debug.WriteLine("No motion detected - Room is vacant");
                    roomstatus = "Vacant";
                }
                #endregion

                await SendDeviceToCloudMessageAsync(roomstatus, bmpSensorData, adcSensorData);

                await Task.Delay(10000);

            }

           // Once the asynchronous method(s) are done, close the deferral.
           // deferral.Complete();
        }

        #region Private methods
        private void InitGPIO()
        {
            // get the GPIO controller
            var gpio = GpioController.GetDefault();
            

            // return an error if there is no gpio controller
            if (gpio == null)
            {
                led = null;
                Debug.WriteLine("There is no GPIO controller.");
                return;
            }

            Debug.WriteLine("GPIO is Ready. Pin Count = " + gpio.PinCount);

            // set up the LED on the defined GPIO pin
            // and set it to High to turn off the LED
            led = gpio.OpenPin(ledPin);
            led.Write(GpioPinValue.High);
            led.SetDriveMode(GpioPinDriveMode.Output);

            // set up the PIR sensor's signal on the defined GPIO pin
            // and set it's initial value to Low
            pir = gpio.OpenPin(pirPin);
            pir.SetDriveMode(GpioPinDriveMode.Input);

            Debug.WriteLine("GPIO pins initialized correctly.");
        }
        private MCP3008SensorData ReadLightStatusInRoom()
        {
            var MCP3008SensorData = new MCP3008SensorData();
            try
            {
                if (mcp3008 == null)
                {
                    Debug.WriteLine("Light Sensor data is not ready");
                    MCP3008SensorData.lightStatus = "N/A";
                    return MCP3008SensorData;
                }

                // The new light state, assume it's just right to start.
                eState newState = eState.JustRight;

                // Read from the ADC chip the current values of the two pots and the photo cell.
                MCP3008SensorData.lowPotReadVal = mcp3008.ReadADC(LowPotentiometerADCChannel);
                MCP3008SensorData.highPotReadVal = mcp3008.ReadADC(HighPotentiometerADCChannel);
                MCP3008SensorData.cdsReadVal = mcp3008.ReadADC(CDSADCChannel);

                // convert the ADC readings to voltages to make them more friendly.
                MCP3008SensorData.lowPotVoltage = mcp3008.ADCToVoltage(MCP3008SensorData.lowPotReadVal);
                MCP3008SensorData.highPotVoltage = mcp3008.ADCToVoltage(MCP3008SensorData.highPotReadVal);
                MCP3008SensorData.cdsVoltage = mcp3008.ADCToVoltage(MCP3008SensorData.cdsReadVal);

                // Let us know what was read in.
                Debug.WriteLine(String.Format("Read values {0}, {1}, {2} ", MCP3008SensorData.lowPotReadVal,
                    MCP3008SensorData.highPotReadVal, MCP3008SensorData.cdsReadVal));
                Debug.WriteLine(String.Format("Voltages {0}, {1}, {2} ", MCP3008SensorData.lowPotVoltage,
                    MCP3008SensorData.highPotVoltage, MCP3008SensorData.cdsVoltage));

                // Compute the new state by first checking if the light level is too low
                if (MCP3008SensorData.cdsVoltage < MCP3008SensorData.lowPotVoltage)
                {
                    newState = eState.TooDark;
                }

                // And now check if it too high.
                if (MCP3008SensorData.cdsVoltage > MCP3008SensorData.highPotVoltage)
                {
                    newState = eState.TooBright;

                }

                // Use another method to determine what to do with the state.
                MCP3008SensorData.lightStatus = CheckForStateValue(newState);
                return MCP3008SensorData;
            }
            catch (Exception)
            {
                MCP3008SensorData.lightStatus = "N/A";
                return MCP3008SensorData;
            }

        }
        private string CheckForStateValue(eState newState)
        {
            String lightStatus;

            switch (newState)
            {
                case eState.JustRight:
                    {
                        lightStatus = JustRightLightString;
                    }
                    break;

                case eState.TooBright:
                    {
                        lightStatus = HighLightString;
                    }
                    break;

                case eState.TooDark:
                    {
                        lightStatus = LowLightString;
                    }
                    break;

                default:
                    {
                        lightStatus = "N/A";
                    }
                    break;
            }

            return lightStatus;
        }
        private async Task SendDeviceToCloudMessageAsync(string status, BMP280SensorData BMP280SensorData, MCP3008SensorData MCP3008SensorData)
        {
            ConferenceRoomDataPoint conferenceRoomDataPoint = new ConferenceRoomDataPoint()
            {
                DeviceId = deviceName,
                Time = DateTime.UtcNow.ToString("o"),
                RoomTemp = BMP280SensorData.Temperature.ToString(),
                RoomPressure = BMP280SensorData.Pressure.ToString(),
                RoomAlt = BMP280SensorData.Altitude.ToString(),
                LightStatus = MCP3008SensorData.lightStatus,
                LightCDSValue = MCP3008SensorData.cdsReadVal.ToString(),
                LightCDSVoltageValue = MCP3008SensorData.cdsVoltage.ToString(),
                RoomStatus = status
            };

            if (status == "Occupied")
            {
                conferenceRoomDataPoint.Color = "Red";
            }
            else
            {
                conferenceRoomDataPoint.Color = "Green";
            }



            var jsonString = JsonConvert.SerializeObject(conferenceRoomDataPoint);
            //var jsonStringInBytes = new Message(Encoding.ASCII.GetBytes(jsonString));

            await AzureIoTHub.SendDeviceToCloudMessageAsync(jsonString);
            Debug.WriteLine("{0} > Sending message: {1}", DateTime.UtcNow, jsonString);
        }
        #endregion
    }
    ```
9. Add a new class to the project by __Right-Clicking__ on the project name in *Solution Explorer* and choosing __Add->Class__. Call the new class __"MCP3008"__. 
10. Add the following code to the new *MCP3008.cs* file replacing the exising class defintion (leave the outer Namespace defintion in place). *The light detector part of the circuit you built earlier produces a variable voltage however the Raspberry PI does not have a built-in Analog-To-Digital converter therefore it needs to use an external MCP3008 chip to do the work. The code you are about to add to the new class file knows how to read data from that chip and convert it into a value that can be used in the rest of the program.*
    ```
 class MCP3008
    {
        // Constants for the SPI controller chip interface
        private SpiDevice mcp3008;
        const int SPI_CHIP_SELECT_LINE = 0;  // SPI0 CS0 pin 24

        // ADC chip operation constants
        const byte MCP3008_SingleEnded = 0x08;
        const byte MCP3008_Differential = 0x00;

        // These are used when we calculate the voltage from the ADC units
        float ReferenceVoltage;
        public const uint Min = 0;
        public const uint Max = 1023;


        public MCP3008(float referenceVolgate)
        {
            Debug.WriteLine("MCP3008::New MCP3008");

            // Store the reference voltage value for later use in the voltage calculation.
            ReferenceVoltage = referenceVolgate;
        }

        /// <summary>
        /// This method is used to configure the Pi2 to communicate over the SPI bus to the MCP3008 ADC chip.
        /// </summary>
        public async void Initialize()
        {
            Debug.WriteLine("MCP3008::Initialize");
            try
            {
                // Setup the SPI bus configuration
                var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
                // 3.6MHz is the rated speed of the MCP3008 at 5v
                settings.ClockFrequency = 3600000;
                settings.Mode = SpiMode.Mode0;

                // Ask Windows for the list of SpiDevices

                // Get a selector string that will return all SPI controllers on the system 
                string aqs = SpiDevice.GetDeviceSelector();

                // Find the SPI bus controller devices with our selector string           
                var dis = await DeviceInformation.FindAllAsync(aqs);

                // Create an SpiDevice with our bus controller and SPI settings           
                mcp3008 = await SpiDevice.FromIdAsync(dis[0].Id, settings);

                if (mcp3008 == null)
                {
                    Debug.WriteLine(
                        "SPI Controller {0} is currently in use by another application. Please ensure that no other applications are using SPI.",
                        dis[0].Id);
                    return;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
                throw;
            }
        }

        /// <summary> 
        /// This method does the actual work of communicating over the SPI bus with the chip.
        /// To line everything up for ease of reading back (on byte boundary) we 
        /// will pad the command start bit with 7 leading "0" bits
        ///
        /// Write 0000 000S GDDD xxxx xxxx xxxx
        /// Read  ???? ???? ???? ?N98 7654 3210
        /// S = start bit
        /// G = Single / Differential
        /// D = Chanel data 
        /// ? = undefined, ignore
        /// N = 0 "Null bit"
        /// 9-0 = 10 data bits
        /// </summary>
        public int ReadADC(byte whichChannel)
        {
            byte command = whichChannel;
            command |= MCP3008_SingleEnded;
            command <<= 4;

            byte[] commandBuf = new byte[] { 0x01, command, 0x00 };

            byte[] readBuf = new byte[] { 0x00, 0x00, 0x00 };

            mcp3008.TransferFullDuplex(commandBuf, readBuf);

            int sample = readBuf[2] + ((readBuf[1] & 0x03) << 8);
            int s2 = sample & 0x3FF;
            Debug.Assert(sample == s2);

            return sample;
        }

        /// <summary>
        ///  Returns the ADC value (uint) as a float voltage based on the configured reference voltage
        /// </summary>
        /// <param name="adc"> the ADC value to convert</param>
        /// <returns>The computed voltage based on the reference voltage</returns>
        public float ADCToVoltage(int adc)
        {
            return (float)adc * ReferenceVoltage / (float)Max;
        }
    }

    public sealed class MCP3008SensorData
    {
        public int lowPotReadVal { get; set; }
        public int highPotReadVal { get; set; }
        public int cdsReadVal { get; set; }
        public float lowPotVoltage { get; set; }
        public float highPotVoltage { get; set; }
        public float cdsVoltage { get; set; }
        public string lightStatus { get; set; }
    }
    ```
11. Add the following *#using* statements to the top of the MCP3008.cs file
    ```
    using Windows.Devices.Enumeration;
    using Windows.Devices.Spi;
    using System.Diagnostics;
    ```
12. Add a new class to the project by __Right-Clicking__ on the project name in *Solution Explorer* and choosing __Add->Class__. Call the new class __"BMP280"__. 
13. Add the following code to the new *BMP280.cs* file replacing the exising class defintion (leave the outer Namespace defintion in place).__Not only are you replacing the class defintion but you are adding two new classes to support it__. *This new class contains code which will read temprature and pressure data from the BMP280 sensor. This is done via the use of the I2C interface*.
    ```
    public sealed class BMP280_CalibrationData
    {
        //BMP280 Registers
        public UInt16 dig_T1 { get; set; }
        public Int16 dig_T2 { get; set; }
        public Int16 dig_T3 { get; set; }

        public UInt16 dig_P1 { get; set; }
        public Int16 dig_P2 { get; set; }
        public Int16 dig_P3 { get; set; }
        public Int16 dig_P4 { get; set; }
        public Int16 dig_P5 { get; set; }
        public Int16 dig_P6 { get; set; }
        public Int16 dig_P7 { get; set; }
        public Int16 dig_P8 { get; set; }
        public Int16 dig_P9 { get; set; }
    }

    public sealed class BMP280
    {
        //The BMP280 register addresses according the the datasheet: http://www.adafruit.com/datasheets/BST-BMP280-DS001-11.pdf
        const byte BMP280_Address = 0x77; // I2C base address
        //const byte BMP280_Signature = 0x58; //BMP280 Device
        //const byte BMP280_Signature = 0x60; // BME280 Device (a BMP280 with addeded Humidity sensor).

        enum eRegisters : byte
        {
            BMP280_REGISTER_DIG_T1 = 0x88,
            BMP280_REGISTER_DIG_T2 = 0x8A,
            BMP280_REGISTER_DIG_T3 = 0x8C,

            BMP280_REGISTER_DIG_P1 = 0x8E,
            BMP280_REGISTER_DIG_P2 = 0x90,
            BMP280_REGISTER_DIG_P3 = 0x92,
            BMP280_REGISTER_DIG_P4 = 0x94,
            BMP280_REGISTER_DIG_P5 = 0x96,
            BMP280_REGISTER_DIG_P6 = 0x98,
            BMP280_REGISTER_DIG_P7 = 0x9A,
            BMP280_REGISTER_DIG_P8 = 0x9C,
            BMP280_REGISTER_DIG_P9 = 0x9E,

            BMP280_REGISTER_CHIPID = 0xD0,
            BMP280_REGISTER_VERSION = 0xD1,
            BMP280_REGISTER_SOFTRESET = 0xE0,

            BMP280_REGISTER_CAL26 = 0xE1,  // R calibration stored in 0xE1-0xF0

            BMP280_REGISTER_CONTROLHUMID = 0xF2,
            BMP280_REGISTER_CONTROL = 0xF4,
            BMP280_REGISTER_CONFIG = 0xF5,

            BMP280_REGISTER_PRESSUREDATA_MSB = 0xF7,
            BMP280_REGISTER_PRESSUREDATA_LSB = 0xF8,
            BMP280_REGISTER_PRESSUREDATA_XLSB = 0xF9, // bits <7:4>

            BMP280_REGISTER_TEMPDATA_MSB = 0xFA,
            BMP280_REGISTER_TEMPDATA_LSB = 0xFB,
            BMP280_REGISTER_TEMPDATA_XLSB = 0xFC, // bits <7:4>

            BMP280_REGISTER_HUMIDDATA_MSB = 0xFD,
            BMP280_REGISTER_HUMIDDATA_LSB = 0xFE,
        };

        //String for the friendly name of the I2C bus 
        const string I2CControllerName = "I2C1";
        //Create an I2C device
        private I2cDevice bmp280 = null;
        //Create new calibration data for the sensor
        BMP280_CalibrationData CalibrationData;
        //Variable to check if device is initialized
        bool init = false;

        //Method to initialize the BMP280 sensor
        public IAsyncAction Initialize()
        {
            return Task.Run(async () =>
            {
                Debug.WriteLine("BMP280::Initialize");

                try
                {
                    //Instantiate the I2CConnectionSettings using the device address of the BMP280
                    I2cConnectionSettings settings = new I2cConnectionSettings(BMP280_Address);
                    //Set the I2C bus speed of connection to fast mode
                    settings.BusSpeed = I2cBusSpeed.FastMode;
                    //Use the I2CBus device selector to create an advanced query syntax string
                    var aqs = I2cDevice.GetDeviceSelector(I2CControllerName);
                    //Use the Windows.Devices.Enumeration.DeviceInformation class to create a collection using the advanced query syntax string
                    DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(aqs);

                    //Instantiate the the BMP280 I2C device using the device id of the I2CBus and the I2CConnectionSettings
                    bmp280 = await I2cDevice.FromIdAsync(dis[0].Id, settings);
                    //Check if device was found
                    if (bmp280 == null)
                    {
                        Debug.WriteLine("BMP280 Device not found");
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
                    throw;
                }
            }).AsAsyncAction();  
        }

        private async Task Begin()
        {
            Debug.WriteLine("BMP280::Begin");
            byte[] WriteBuffer = new byte[] { (byte)eRegisters.BMP280_REGISTER_CHIPID };
            byte[] ReadBuffer = new byte[] { 0xFF };

            //Read the device signature
            bmp280.WriteRead(WriteBuffer, ReadBuffer);
            Debug.WriteLine("BMP280 Signature: " + ReadBuffer[0].ToString());

            ////Verify the device signature
            //if (ReadBuffer[0] != BMP280_Signature)
            //{
            //    Debug.WriteLine("BMP280::Begin Signature Mismatch.");
            //    return;
            //}

            //Set the initalize variable to true
            init = true;

            //Read the coefficients table
            CalibrationData = await ReadCoefficeints();

            //Write control register
            await WriteControlRegister();

            //Write humidity control register
            await WriteControlRegisterHumidity();
        }

        //Method to write 0x03 to the humidity control register
        private async Task WriteControlRegisterHumidity()
        {
            byte[] WriteBuffer = new byte[] { (byte)eRegisters.BMP280_REGISTER_CONTROLHUMID, 0x03 };
            bmp280.Write(WriteBuffer);
            await Task.Delay(1);
            return;
        }

        //Method to write 0x3F to the control register
        private async Task WriteControlRegister()
        {
            byte[] WriteBuffer = new byte[] { (byte)eRegisters.BMP280_REGISTER_CONTROL, 0x3F };
            bmp280.Write(WriteBuffer);
            await Task.Delay(1);
            return;
        }

        //Method to read a 16-bit value from a register and return it in little endian format
        private UInt16 ReadUInt16_LittleEndian(byte register)
        {
            UInt16 value = 0;
            byte[] writeBuffer = new byte[] { 0x00 };
            byte[] readBuffer = new byte[] { 0x00, 0x00 };

            writeBuffer[0] = register;

            bmp280.WriteRead(writeBuffer, readBuffer);
            int h = readBuffer[1] << 8;
            int l = readBuffer[0];
            value = (UInt16)(h + l);
            return value;
        }

        //Method to read an 8-bit value from a register
        private byte ReadByte(byte register)
        {
            byte value = 0;
            byte[] writeBuffer = new byte[] { 0x00 };
            byte[] readBuffer = new byte[] { 0x00 };

            writeBuffer[0] = register;

            bmp280.WriteRead(writeBuffer, readBuffer);
            value = readBuffer[0];
            return value;
        }

        //Method to read the caliberation data from the registers
        private async Task<BMP280_CalibrationData> ReadCoefficeints()
        {
            // 16 bit calibration data is stored as Little Endian, the helper method will do the byte swap.
            CalibrationData = new BMP280_CalibrationData();

            // Read temperature calibration data
            CalibrationData.dig_T1 = ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_T1);
            CalibrationData.dig_T2 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_T2);
            CalibrationData.dig_T3 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_T3);

            // Read presure calibration data
            CalibrationData.dig_P1 = ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P1);
            CalibrationData.dig_P2 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P2);
            CalibrationData.dig_P3 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P3);
            CalibrationData.dig_P4 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P4);
            CalibrationData.dig_P5 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P5);
            CalibrationData.dig_P6 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P6);
            CalibrationData.dig_P7 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P7);
            CalibrationData.dig_P8 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P8);
            CalibrationData.dig_P9 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BMP280_REGISTER_DIG_P9);

            await Task.Delay(1);
            return CalibrationData;
        }

        //t_fine carries fine temperature as global value
        Int32 t_fine = Int32.MinValue;
        //Method to return the temperature in DegC. Resolution is 0.01 DegC. Output value of “5123” equals 51.23 DegC.
        private double BMP280_compensate_T_double(Int32 adc_T)
        {
            double var1, var2, T;

            //The temperature is calculated using the compensation formula in the BMP280 datasheet
            var1 = ((adc_T / 16384.0) - (CalibrationData.dig_T1 / 1024.0)) * CalibrationData.dig_T2;
            var2 = ((adc_T / 131072.0) - (CalibrationData.dig_T1 / 8192.0)) * CalibrationData.dig_T3;

            t_fine = (Int32)(var1 + var2);

            T = (var1 + var2) / 5120.0;
            return T;
        }


        //Method to returns the pressure in Pa, in Q24.8 format (24 integer bits and 8 fractional bits).
        //Output value of “24674867” represents 24674867/256 = 96386.2 Pa = 963.862 hPa
        private Int64 BMP280_compensate_P_Int64(Int32 adc_P)
        {
            Int64 var1, var2, p;

            //The pressure is calculated using the compensation formula in the BMP280 datasheet
            var1 = t_fine - 128000;
            var2 = var1 * var1 * (Int64)CalibrationData.dig_P6;
            var2 = var2 + ((var1 * (Int64)CalibrationData.dig_P5) << 17);
            var2 = var2 + ((Int64)CalibrationData.dig_P4 << 35);
            var1 = ((var1 * var1 * (Int64)CalibrationData.dig_P3) >> 8) + ((var1 * (Int64)CalibrationData.dig_P2) << 12);
            var1 = (((((Int64)1 << 47) + var1)) * (Int64)CalibrationData.dig_P1) >> 33;
            if (var1 == 0)
            {
                Debug.WriteLine("BMP280_compensate_P_Int64 Jump out to avoid / 0");
                return 0; //Avoid exception caused by division by zero
            }
            //Perform calibration operations as per datasheet: http://www.adafruit.com/datasheets/BST-BMP280-DS001-11.pdf
            p = 1048576 - adc_P;
            p = (((p << 31) - var2) * 3125) / var1;
            var1 = ((Int64)CalibrationData.dig_P9 * (p >> 13) * (p >> 13)) >> 25;
            var2 = ((Int64)CalibrationData.dig_P8 * p) >> 19;
            p = ((p + var1 + var2) >> 8) + ((Int64)CalibrationData.dig_P7 << 4);
            return p;
        }

/*
 * 
 *  Public methods to read the data
 * 
 * 
 */
        public IAsyncOperation<float> ReadTemperature()
        {
            return AsyncInfo.Run(async _ => (float)await ReadTemperatureAsync());
        }
        private async Task<float> ReadTemperatureAsync()
        {
            //Make sure the I2C device is initialized
            if (!init) await Begin();

            //Read the MSB, LSB and bits 7:4 (XLSB) of the temperature from the BMP280 registers
            byte tmsb = ReadByte((byte)eRegisters.BMP280_REGISTER_TEMPDATA_MSB);
            byte tlsb = ReadByte((byte)eRegisters.BMP280_REGISTER_TEMPDATA_LSB);
            byte txlsb = ReadByte((byte)eRegisters.BMP280_REGISTER_TEMPDATA_XLSB); // bits 7:4

            //Combine the values into a 32-bit integer
            Int32 t = (tmsb << 12) + (tlsb << 4) + (txlsb >> 4);

            //Convert the raw value to the temperature in degC
            double temp = BMP280_compensate_T_double(t);

            //Return the temperature as a float value
            return (float)temp;
            }


        public IAsyncOperation<float> ReadPreasure()
        {
            return AsyncInfo.Run(async _ => (float)await ReadPreasureAsync());
        }
        private async Task<float> ReadPreasureAsync()
        {
            //Make sure the I2C device is initialized
            if (!init) await Begin();

            //Read the temperature first to load the t_fine value for compensation
            if (t_fine == Int32.MinValue)
            {
                await ReadTemperature();
            }

            //Read the MSB, LSB and bits 7:4 (XLSB) of the pressure from the BMP280 registers
            byte tmsb = ReadByte((byte)eRegisters.BMP280_REGISTER_PRESSUREDATA_MSB);
            byte tlsb = ReadByte((byte)eRegisters.BMP280_REGISTER_PRESSUREDATA_LSB);
            byte txlsb = ReadByte((byte)eRegisters.BMP280_REGISTER_PRESSUREDATA_XLSB); // bits 7:4

            //Combine the values into a 32-bit integer
            Int32 t = (tmsb << 12) + (tlsb << 4) + (txlsb >> 4);

            //Convert the raw value to the pressure in Pa
            Int64 pres = BMP280_compensate_P_Int64(t);

            //Return the temperature as a float value
            return ((float)pres) / 256;
        }

        ////Method to take the sea level pressure in Hectopascals(hPa) as a parameter and calculate the altitude using current pressure.
        //private async Task<float> ReadAltitudeAsync(float seaLevel)
        //{
        //    //Make sure the I2C device is initialized
        //    if (!init) await Begin();

        //    //Read the pressure first
        //    float pressure = await ReadPreasureAsync();
        //    //Convert the pressure to Hectopascals(hPa)
        //    pressure /= 100;

        //    //Calculate and return the altitude using the international barometric formula
        //    return 44330.0f * (1.0f - (float)Math.Pow((pressure / seaLevel), 0.1903f));
        //}
    }

    public sealed class BMP280SensorData
    {
        public float Temperature { get; set; }
        public float Pressure { get; set; }
        public float Altitude { get; set; }
    }
    ```
14. Add the following *#using* statements to the top of the BMP280.cs file
    ```
    using Windows.Devices.Enumeration;
    using Windows.Devices.Gpio;
    using Windows.Devices.I2c;
    using Windows.Foundation;
    using Windows.Storage;
    ```
15. Add one more class to the project called *ConferenceRoomDataPoint*. Replace the class definition with the following code:-
    ```
        public sealed class ConferenceRoomDataPoint
    {
        public string DeviceId { get; set; }
        public string RoomStatus { get; set; }

        public string RoomTemp { get; set; }

        public string RoomPressure { get; set; }

        public string RoomAlt { get; set; }
        public string Color { get; set; }
        public string Time { get; set; }
        public string LightStatus { get; internal set; }
        public string LightCDSValue { get; internal set; }
        public string LightCDSVoltageValue { get; internal set; }
    }
    ```
16. Open the *AzureIoTHub.cs* file added by the wizard earlier. Replace the deviceConnectionString with the one you saved in __Step 3__ of the *Step 2 - Register your device with IoT Hub* section.
17. At this point all the code is ready and your circuit should also be ready. Time to test! You can now deploy and test the application by pressing `F5`. *The first time you deploy an application it make take some time as required framework updates are installed onto the device - Visual Studio may even display some "This is taking too long messages" - Be patient! Subsequent deployments will be much quicker.*. For help on deploying apps please see [Deploying an App with Visual Studio](https://developer.microsoft.com/en-us/windows/iot/Docs/appdeployment)

Step 4 - Verify your device is correctly collecting data.
=========================================================

Once the UWP App is deployed to your device it will run and collect data every 10 seconds *(this can be changed by updating the looping delay at line 123 of StartupTask.cs)*.

When the UWP application and external circuit are both correctly built and configured, you will see the following output in the Debug window of Visual Studio:-

![Debug output when device, application and circuit are working correctly](images/workingdebugoutput.png)

The following items are of note:-
* You will see the date the reading was taken.
* The temperature in deg. C.
* The pressure (three different units are displayed).
* Two lines which display the values from the Analog-to-Digital conversion MCP3008 chip.
* The light status in the room (dark/bright/too bright).
* The motion detector status indicating wether the room is occupied or not.
* A line containing the string "Sending Message" with JSON data. This is the data being uploaded to IoT Hub.

Step 5 - Use the Device Explorer to verify your device is correctly sending data.
=================================================================================

The Device Explorer tool which you have already used previously can be used to monitor messages which have been sent to IoT Hub. We are going to use it to confirm our data arrived correctly by reading the data back via IoT Hub's Event Hub compatible endpoint.

1.	Open the Device Explorer (*C:\Program Files (x86)\Microsoft\DeviceExplorer\DeviceExplorer.exe*) and fill the IoT Hub Connection String field with the connection string of the IoT Hub you created in previous steps and click on Update.
    ![Setting the connection string](images/deconfigure.png)
2. Click on the `Data` tab, ensure that your device is selected in the `Device ID` drop down, then __Click__ the  __Monitor__ button. You should see a new JSON string every 10 seconds which contains the data from your device.
    ![Monitor messages sent to IoT Hub](images/monitormessages.png)
 
Step 6 - Open the provided Client UWP App to visualize the data
===============================================================

To provide a more visually attractive way of viewing the data, a standard UWP App has been created for you to visualise the data. This will connect to the Event Hub compatible endpoint associated with your IoT Hub and read the messages.

1. Open the [Client App](source/ClientApp/ClientApp.sln) application in another instance of Visual Studio.
2. Open the MainPage.xaml.cs file.
3. On line 38 (the *ConnectionString* variable) remove the reference to the service bus endpoint, i.e. `sb://ihsuprodamres036dednamespace.servicebus.windows.net/` and replace it with your own __Event Hub-compatible endpoint__ as recorded in the *Step 1 - Build an IoT Hub* section above.
4. Again on the same line, replace the *iothubowner* Shared Secret with the value previously saved.
5. On line 39 (the *eventHubEntity* variable) remove the existing value and replace it with your own __Event Hub-compatible name__ as recorded in the *Step 1 - Build an IoT Hub* section above.
6. The completed two lines will now look similar to this:-
```
        static string ConnectionString = "Endpoint=sb://ihsuprodamres034dednamespace.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=kWdvdpkc493bhytnV9dun/le7k0MsBcIjFpssw=";
```
``` 
        static string eventHubEntity = "iothub-ehub-deviothub9-88974-af8949fa5d";
```

__Now press `F5` to run the application__. Note this runs on your local Windows 10 development machine and not on your IoT device, so don't try to deploy it there.

The running application will display the room occupied status, temperature and pressure data which is being sent from the Windows 10 IoT device.

![Room Status Application](images/roomstatusapp.png).

Step 7 - Configure Azure Stream Analytics to pull data from IoT Hub
===================================================================

You will now configure Azure Stream Analytics to pull the uploaded data from IoT Hub, compute a set of rolling averages then off-load the data to Azure PowerBI.

1. Log in to the [Azure classic portal](http://manage.windowsazure.com)
2. Click on "__New plus__" icon on bottom left hand corner of the page then __Select Data Services->Stream Analytics->Quick Create__
3. ![Stream Analytics->Quick Create](images/streamanalyticsquickcreate.png)
4. Enter a *Job Name* of `Rooms`, Select `Region` and a related regional storage account (or create a new storage account if one does not exist already).
5. Finally __Click__ the *Create Stream Analytics Job* button.
6. ![Creating the Stream Analytics Job](images/createsajob.png)
7. Select the new stream analytics job, click the *Inputs* tab and select *Add An Input*.
8. ![Add a Stream Analytics Input](images/addinput.png)
9. Select the `Add Data Stream` option then press the `Next` arrow.
10. ![Choosing to add a Data Stream](images/adddatastream.png)
11. Select IoT Hub as the data stream input and click `Next`.
12. ![Choosing to add a Data Stream](images/selectionhub.png)
13. Enter an Input stream alias name of `RoomsDataIn`. This will be used in later steps.
14. Select a subscription then choose the IoT Hub which you created above.
15. Select `iothubowner` as shared access policy name and click on next.
16. ![IoT Hub Settings for Stream Analytics Job Input](images/iothubsettings.png)
17. Select Event serialization format as JSON and Encoding as UTF8 then __Click__ the *Tick icon* to create the input.
18. ![Serialization Settings](images/serializationsettings.png)
19. The completed input looks like this:
20. ![Completed input](images/completedinput.png)
21.  Click on the `Outputs` tab then __Click__ *Add an Output*.
22. ![Add Output](images/addoutput.png)
23. Choose `PowerBI` as the destination and click `Next`.
24. ![Choose BI Output](images/choosebi.png)
25. To use *Power BI* you must authorize the Stream Analytics service to access your companies Power BI subscription in order to define a new datasheet and send across data. __Click Authorise Now__. *You can register for a free PowerBI account but your company must be an existing O365 customer or you must have setup an O365 trial*.
27. ![Authorizing the Stream Analytics Service to connect to your PowerBI subscription](images/authorizepowerbi.png)
28. After successful authorization, you should enter an Output alias name (e.g. `BIOut`) which is a friendly name to reference in output queries.
29. Enter `RoomSet` as the DataSet Name and `Rooms` as the table name (you can only have one table in a Stream Analytics output dataset)
30. Select *My Workspace* as the workspace.
31. ![Stream Analytics Output Job Creation Settings](images/outputconfiguresettings.png)
32. The completed output looks like this:
33. ![Completed output](images/completedoutput.png)
34. Click the `Query` tab and in the *Query* section of the screen enter the Stream Analytics query shown below. *Note that if you have given different names to your Input and Output locations you will have to change the query accordingly.*
    ```
    SELECT
    DeviceId,
    RoomStatus,
    LightStatus,
    Max(Time) Time,
    Avg(RoomTemp) roomtempAVG,
    Avg(RoomPressure) AVGPressure,
    Avg(RoomAlt) AVGRoomAlt,
    Avg(LightCDSValue) AVGLightCDSValue,
    Avg(LightCDSVoltageValue) LightCDSVoltageValue
INTO
    [BIOut]
FROM
    [RoomsDataIn] TIMESTAMP by Time
GROUP BY
    DeviceId,RoomStatus,LightStatus,TumblingWindow(Second,10)
    ```

35. Save work work by pressing __Save__ at the bottom of the page.
36. Finally run the job by clicking on the __Start__ button.


Step 8 - Configure PowerBI to display the average temperature
=============================================================

Finally you will use PowerBI to display a guage showing the average temperature of the room. Once the Stream Analytics job has started a new dataset called `RoomSet` which contains a single table called `Rooms` will be available in PowerBI. You will display data from this table.

__Note:__ *At time of writing the ability to display the latest values of temperature, pressure, light status etc. coming from Stream Analytics is in private preview therefore the report below will display an "average" of "averages", i.e. an average of all the temprature values stored. Once the ability to display the latest known value is made available, this section of the scenario will be updated. You can find out more about these upcoming features at https://powerbi.microsoft.com/en-us/blog/announcing-private-preview-of-azure-stream-analytics-outputting-to-power-bi-streaming-datasets/*

1. Login to the [PowerBI](https://www.powerbi.com) site.
2. Locate the `Streaming DataSets` dataset from the bottom of the hand menu and __Click__ it.
    ![Start Stream Analytics Job](images/pbidataset.png).
4. Click on the + icon to create a report against the *RoomSet* dataset. This displays the report canvas, the data table inside the dataset and the different visualizations available.
    ![Start Stream Analytics Job](images/addingtodataset.png).
5. Click to add a *Guage chart*.
    ![Adding guage chart](images/vizualization.png).
6. Drag and drop the `roomtempavg` value field from fields tab to `value` field in Visualization tab.
    ![Setting the visualization value](images/vizvalue.png).
7. The guage now looks like this:-
    ![Visualization value result](images/guage.png).
8. Change the `roomtempavg` value from `sum` to `average`.
9. ![Setting the visualization value to show average data](images/vizaverage.png).
10. The final visualization shows the average of all the temprature values it has received from Stream Analytics.
11. ![The Final Visualization](images/visualfinal.png).
12. Click on the Pin symbol at the top of the chart to add it to a dashboard. Enter a name for the dashboard then press __Save and Continue__.
    ![Saving the report](images/savereport.png).




