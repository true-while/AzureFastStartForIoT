# Management System and Inventory

![](images/th.jpg)

Scenario
========

In this scenario you will imagine that you are building a system to support a stocking taking programme in a warehouse.
Employees will scan the tags of stock items before storing them in known locations.
This information will be uploaded to IoT Hub and stored in a database for reporting purposes.

Credit for this project is as follows:-

* [Microsoft Premier Services](https://www.microsoft.com/en-us/microsoftservices/support.aspx).

Architecture
============

![Architecture](images/architecture.png)

1. An RFiD Reader will scan a tagged item then send the details to an IoT Hub. Each reader will be registered as a unique device.
2. A Stream Analytics job will remove the scanned data and move it to an Event Hub. Currently nothing is done to the data but in a later update to this scenario this will change.
3. The data is uploaded to a regular Event Hub where it is queued for processing.
4. An Azure Function is configured to trigger off the arrival of a message at the Event Hub.
5. The details of the scanned item will be inserted/updated in an Azure hosted SQL Database.
6. PowerBI will be used to build a quick and simple report to display the stock inventory details from the backend database.

Basic Hardware Setup
====================

As described in the [Kit List for these scenarios](/Electronics/Kit%20List.md), you will need the following hardware items to build this scenario:

* [Microsoft IoT Pack for Raspberry Pi 3 - w/ Raspberry Pi 3](https://www.adafruit.com/products/2733)
* [1 x USB cable - A/MicroB - 3ft](https://www.adafruit.com/product/592)
* [1 x Adafruit Assembled Pi Cobbler Breakout + Cable for Raspberry Pi - Model B](https://www.adafruit.com/product/914)
* __TODO - Add RFID Components here __

Specifically the following items are required from the kit list:

* Raspberry Pi
* Breadboard (generic)
* __TODO - Add RFID Components here __

## Basic Circuit

Build your basic cicuit according to this diagram:

![Breadboard_Model](images/xxxxxxxx.png)

For interest/reference, these are the PINs on the Raspberry Pi.

![Raspberry Pi 3 PinOut Reference](/RaspberryPI/images/pinout.png)

## "Here's one I made earlier"

![](images/xxxxxxxx.png)

Azure Pre-reqs
==============

1. A working Azure subscription or trial - http://portal.azure.com
2. A working PowerBI subscription or trial - http://www.powerbi.com
3. The PowerBI Desktop tool must be downloaded from https://powerbi.microsoft.com/en-us/desktop/ and installed on your development machine.

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

You are going to start by building an IoT Hub that hand RFiD scanners carried by employees will upload their data to.

1. [Open the Azure Portal](https://portal.azure.com).
2. Click (+)-->Internet of Things-->IoT Hub.
3. ![New IoT Hub](images/newiot.png)
4. Enter a unique name for the IoT Hub, choose a Pricing and Scale tier (note that Free has been choosen here), select or create a Resource Group and datacentre location and __Click Create__.
5. ![Choosing IoT Hub settings](images/newiothubsettings.png)
6. Once the IoTHub has been created, ensure you make a copy of the *iothubowner* Connection String - this is shown via the *Shared Access Policies-->iothubowner* blade.
7. . ![Iot Hub Key](images/iothubkeys.png)  
8. Also make a copy of the *Event Hub-compatible name* & *Event Hub-compatible endpoint* values. You'll need these later on when you start to read data back from IoT Hub.
9. ![Event Hub Compatible Endpoint](images/eventhubendpoint.png)
10. Finally via the Messaging blade, create a new Consumer Group called __dbstore__, then press __Save__. Consumer groups allow multiple applications to each receive their own copy of the messages sent to the IoTHub.
11. ![Creating a Consumer Group](images/iotconsumergroup.png)

Step 2 - Register your device with IoT Hub
==========================================

For your device to connect to IoT Hub it must have its own Device Identity (aka set of credentials). The process of obtaining these is known as Registering your Device. Currently there is no way to do this via the Azure Portal but there is a remote API available. Rather than write a custom application to connect & register you are going to use Device Explorer which is part of the IoT SDK. You can also register a device via the IoT Dashboard application or use iothub-explorer, another tool from the IoT SDK written in node.js.

1.	Open the Device Explorer (*C:\Program Files (x86)\Microsoft\DeviceExplorer\DeviceExplorer.exe*) and fill the IoT Hub Connection String field with the connection string of the IoT Hub you created in previous steps and click on __Update__.
2. ![Setting the connection string](images/deconfigure.png)
3. Go to the __Management tab__ and __Click on the Create button__. The Create Device popup will be displayed. Enter "__device1__" as the Device ID for your device  and __click on Create__. *The device name is important as other parts of the supplied sample program rely on this*.
4. ![Create device entry](images/createentry.png)
5. Once the device identity is created, it will be displayed in the grid. __Right click__ on the device entry you just created and select __Copy connection string for the selected device__. Paste this into a notepad as it will be required later on.
6. ![Copy device details](images/degrid.png)

__Note__: The device identities registration can be automated using the Azure IoT Hubs SDK. An example can be found at https://azure.microsoft.com/en-us/documentation/articles/iot-hub-csharp-csharp-getstarted/#create-a-device-identity. 

Step 3 - Create an Event Hub
============================

The Event Hub will act as an intermediatory for scanned and uploaded data before it gets processed by the Azure Function.

1. Click (+)-->Internet of Things-->Event Hub.
2. Enter a unique name for the Service Hub namespace, choose a Pricing tier, select or create a Resource Group and datacentre location and __Click Create__.
3. ![Choosing Event Hub settings](images/newnamespace.png)
4. Once the Event Hub has been created, from it's settings blade click on __+New Event Hub__.
5. ![New Event Hub](images/neweventhub.png)
6. Once the Event Hub has been created, ensure you make a copy of the *RootManageSharedAccessKey* Connection String Primary Key- this is shown via the *Shared Access Policies-->RootManageSharedAccessKey* blade.
7. ![Event Hub Key](images/eventhubkey.png)

Step 4 - Stream Analytics Job
=============================

The role of the Stream Analytics is to copy messages unchanged from the IoT Hub to the Event Hub. Later this will be modified to send messages to two different locations.

1. Click (+)-->Internet of Things-->Stream Analytics Job.
2. Call the job "ProcessRFID", select a Resource Group and click __Create__.
3. Once the Stream Analytics Job has been created, click on it's homepage, then click __Inputs__, then click __Add__.
4. ![New Input](images/streamanalyticshome.png)
5. Enter the values shown below to define where Stream Analytics will obtain its Input data from. *You will select the name of the IoT Hub you created in a previous step*. Click __Create__ when you are done.
6. ![New Input Details](images/newinputdetails.png)
7. Back on the Stream Analytics job homepage, click __Outputs__ then __Add__ to define an output location for the job.
8. Enter the values shown below to define where Stream Analytics will obtain its Output data from. *You will select the name of the Service Bus Namespace and Event Hub you created in a previous step*. Note that *Partition Key Column* is left blank. Click __Create__ when you are done.
9. ![New Output Details](images/newoutputdetails.png)
10. To ensure messages from passed from the IoT Hub to the Event Hub, you will need to define a query. From the job's homepage, click __Query__ which is sandwiched between Inputs and Outputs which you clicked earlier.
11. Enter the following query:-
        ```
        SELECT
         *
        INTO
            [EHOut]
        FROM
            [IoTHub]
        ```
13. ![Stream Analytics Query](images/saquery.png)
14. Back on the Stream Analytics Jobs's homepage, click __Start__ at the top of the blade. This will ensure messages are copied from input to output.


Step 5 - Create an Azure Database
=================================

The details on how to create a new Azure SQL Database are very well documented and won't be copied here, instead you should browse to:

`https://docs.microsoft.com/en-us/azure/sql-database/sql-database-get-started`

and follow the instructions under the section __Create a new logical SQL server in the Azure portal__. You can call the logical database server whatever you want, just keep a record of the name and credetials your create for later. You don't need to create a new database as this will be done for you later by the Entity Framework however you will need to keep a record of the username and password.

Once your logical server has been created, you can build a connection string like this (replacing the server name, username and password placeholders with your own details).

`Server=tcp:YOUR_SERVER_NAME_HERE.database.windows.net,1433;Database=RFIDStock;User ID=YOUR_LOGIN_NAME_HERE;Password=YOUR_PASSWORD_HERE;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30`

Make a note of this completed connection string as you'll need it next.

Step 6 - Setup an Azure Function to process incomming data
==========================================================

Azure functions are background jobs that run on web servers and process data. You are going to use one to read the data which has been sent to the IoT Hub then upload it into the database.

1. Open a browser at head to the [Azure Functions page](http://functions.azure.com).
2. __Click__ on the "Login to your account" link under the "Try it for Free" green button.
3. Enter a suitable *name* for the function, *a region*, then click *"Create"*.
    ![Creating an Azure Function](images/createfunction.png).
4. Next.

__TODO ******************************__
__TODO ******************************__
__TODO ******************************__
__TODO ******************************__
__TODO ******************************__
__TODO ******************************__
__TODO ******************************__
__TODO ******************************__


https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-event-hubs

http://stackoverflow.com/questions/40671391/azure-functions-with-entity-framework



Step 7 - Build an application to upload the data
================================================

In this section you will build an application for your Windows 10 IoT device. This will emulate a typical hand help RFiD scanner that an operative in a warehouse would carry. Eachtime an item with an attached RFiD sticker is scanned, a small packet of data indicating the id, location and time/date will be uploaded to IoT Hub.

A [Completed Example](source/XXXXXX) is also available. __TODO__: Add details on how to set this up and use it.

1. Open Visual Studio then go *File->Project->Visual C#->Windows->Windows IoT Core* and select the *Background Application (IoT)* template.
    ![Blank project](images/newproject.png)
2. Call your project "StockBackApp" and make sure the .NET Framework version is 4.5.1 or later. Click __Create__. Accept the defaults for Universal Windows Project target versions.
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


__TODO ******************************__
__TODO ******************************__
__TODO ******************************__
__TODO ******************************__
__TODO ******************************__
__TODO ******************************__
__TODO ******************************__ 

Step 7 - View the captured data
================================

In order to view the captured data, you will use PowerBI to build a report which displays the current and historical locations of items from the warehouse.

1. Open PowerPI Desktop and click on __Get Data__.
    ![Get Data](images/getdata.png).
2. Select *Azure->Microsoft Azure SQL Database* and press __Connect__
    ![Choose Database](images/azuresqldb.png).
3. Enter your Azure logical SQL Server name as created in a previous step.
     ![Enter DB Name](images/sqlservername.png).
4. Select the "StockItems" table, then click __Load__.
     ![Select Table](images/selecttable.png).

You now have the data source added, all that remains is to display the data. In this simple example you simply display all the data in the database however the scenario could be extened to show only the most recent location.

5. Click on the *Name* column box under the StockItems table on the Fields pane. This will add it to a report on the main report canvas. If there is any data in the database it will be queriered now and displayed.
     ![Select Table](images/addname.png).
6. Now add ticks to the *LastSeen*, *Location* and *RFiD* columns.

The final report looks similar to this:

![Final Report](images/finalreport.png).





















