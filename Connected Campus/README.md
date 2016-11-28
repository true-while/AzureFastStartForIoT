# Connected Campus

![](images/meetingroom.png)

Scenario
========

In this scenario you will build an application for a Windows 10 Core device with several attached sensors.
Every 10 seconds it will send information about the environmental conditions of a room in which it is located to Azure.
This data will then be displayed on a dashboard where the user can see if the room is free and wether they wish to use it or not.

Credits: Full credits for this scenario go to:

* [Intelligent Multi-Conference Room using UWP App and Azure](https://microsoft.hackster.io/en-US/Kishore10211/intelligent-multi-conference-room-using-uwp-app-and-azure-620b1d) - Made by [Kishore Gaddam](https://microsoft.hackster.io/en-US/Kishore10211).

Additional content has been added by

* [Microsoft Premier Services](https://www.microsoft.com/en-us/microsoftservices/support.aspx).

Architecture
============

The system you are about to build consists of the following parts/will work in the following manner:

1. __An IoT Hub__. This will act as the main gateway for ingesting data from connected devices. You will need to register your device with IoT Hub before you can send it data.
2. A __Raspberry Pi__ with attached sensors. A motion sensor will be used determine wether anybody is in the room with light and temprature data being collected via appropriate sensors.
3. The collected data will be uploaded to IoTHub.
4. An application will be created to allow the viewing of the raw data once it has been uploaded.
5. The raw data will be feed into __Azure Stream Analytics__ to produce averages of temprature overtime.
6. A copy of the processes data will be archived into an Azure Storage Account.
7. The processed data will be filtered to suit our business needs.
8. The final data will be sent to __Power BI__ for display. This will make it easly available to view on a number of mobile devices or computers.

![](images/overview.png)

Basic Hardware Setup
====================

__TODO - This section needs reviewing and updating__

You will need the following hardware items to build this scenario:

* Raspberry Pi
* Breadboard (generic)
* PIR Motion Sensor
* Adafruit BMP280 Barometric Pressure & Altitude Sensor
* MCP3008 - 8-Channel 10-Bit ADC With SPI Interface
* Potentiometer - 10K
* Resistor 10k ohm
* An LED
* Male/Male Jumper Wires
* Female/Male Jumper Wires

![](images/circuit.png)

Azure Pre-reqs
==============

1. XXX
2. XXX

Develoment Machine and IoT device Setup
========================================

* Ensure your local development machine is setup according to these instructions: [Azure IoT Development machine setup](../IoT Developer Setup.docx?raw=true)
* Part of the above document describes installing the "Device Explorer" tool - make sure that you follow these instructions as you'll need that tool later on.
* Ensure you have [followed the instructions](https://developer.microsoft.com/en-us/windows/iot/docs/iotdashboard) to __Use the Windows 10 IoT Core Dashboard__ to setup your Raspberry Pi.

*Note about setting up Wi-Fi on the RPi:* IoT Dashboard shows all available networks that your PC has previously connected to. If you don’t see your desired Wi-Fi network on the list, ensure you’re connected to it on your PC. If you uncheck the Wi-Fi box when writing the Windows 10 IoT Core image onto your SD card, you must connect an Ethernet cable to your board after flashing. Wether using Wi-Fi or Ethernet, your device will recieve an IP via DHCP at start up.

For more in depth guidance on setting up your hardware device see [Get Started](https://developer.microsoft.com/en-us/windows/iot/GetStarted).

Step 1 - Build an IoT Hub
=========================

1. [Open th Azure Portal](https://portal.azure.com).
2. Click (+)-->Internet of Things-->IoT Hub.
3. ![New IoT Hub](images/newiot.png)
4. Enter a unique name for the IoT Hub, choose a Pricing and Scale tier (note that Free has been choosen here), select or create a Resource Group and datacentre location and __Click Create__.
5. ![Choosing IoT Hub settings](images/newiothubsettings.png)
6. Once the IoTHub has been created, ensure to make a copy of the Connection String - this is shown via the *Shared Access Policies-->iothubowner* blade.
7. ![Iot Hub Key](images/iothubkeys.png)

Step 2 - Register your device with IoT Hub
==========================================

For your device to connect to IoT Hub it must have its own Device Identity (aka set of credentials). The process of obtaining these is known as Registering your Device. Currently there is no way to do this via the Azure Portal but there is a remote API available. Rather than write a custom application to connect & register you are going to use Device Explorer which is part of the IoT SDK. You can also register a device via the IoT Dashboard application or use iothub-explorer, another tool from the IoT SDK written in node.js.

1.	Open the Device Explorer (*C:\Program Files (x86)\Microsoft\DeviceExplorer\DeviceExplorer.exe*) and fill the IoT Hub Connection String field with the connection string of the IoT Hub you created in previous steps and click on Update.
2. ![Setting the connection string](images/deviceexplorerconnstr.png)
3. Go to the __Management tab__ and __Click on the Create button__. The Create Device popup will be displayed. Make up a Device ID for your device (myFirstDevice for example) and __click on Create__.
4. ![Create device entry](images/createentry.png)
5. Once the device identity is created, it will be displayed in the grid. Right click on the identity you just created, select __Copy connection string__ for selected device and take note of the value copied to your clipboard, since it will be required to connect your device with the IoT Hub.
6. ![Copy device details](images/degrid.png)

__Note__: The device identities registration can be automated using the Azure IoT Hubs SDK. An example can be found at https://azure.microsoft.com/en-us/documentation/articles/iot-hub-csharp-csharp-getstarted/#create-a-device-identity. 


Step 3 - Create an UWP App for your device
==========================================

This application is going to read the sensor date from your device and upload it to IoT Hub

1. From Visual Studio select File-->New-->Project from the main menu. Create a new project by selecting Universal –> Blank App template. Call it "__ConnectedCampus__".
2. ![Blank project](images/newproject.png).
3. 

######################


[Deploying an App with Visual Studio](https://developer.microsoft.com/en-us/windows/iot/Docs/appdeployment)