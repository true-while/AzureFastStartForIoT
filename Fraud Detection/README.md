# Fraud Detection Scenario

<img src="https://hackster.imgix.net/uploads/cover_image/file/66861/SecurityCamera2.JPG?auto=compress%2Cformat&w=400">
<img src="https://hackster.imgix.net/uploads/cover_image/file/91527/project%20picture.png?auto=compress%2Cformat&w=400">

Scenario
========

For this project you will attach a regular USB camera to your Windows IoT Core device along with a PIR sensor (the required hardware will have been supplied to you by your instructor).
This basic system will detect movement near the camera and trigger it to take a photograph. The image will be uploaded to Azure where Cortana Analytics will analyse the photo.
 __TODO: What will it do with it?__

Basic Hardware Setup
====================

1. Set up your PC and RPi according to these [instructions](http://ms-iot.github.io/content/en-US/win10/SetupPCRPI.htm). Make a note of the IP address of the device, you'll need it for later.
2. Wire the PIR sensor as shown in the image below being sure to use 10 kΩ pull-up resistor.

![Connect the following using a 10 kΩ pull-up resistor.](https://hackster.imgix.net/uploads/image/file/68626/PIR_bb.png?auto=compress%2Cformat&amp;w=680&amp;h=510&amp;fit=max "Connect the following using a 10 kΩ pull-up resistor.")

Azure Pre-reqs
==============

1. Create a new Azure Storge account via the https://portal.azure.com portal.
2. Open the settings for the new storage account and make a note of it's name and access key.
3. Create a new container in the Storage Account called *secuirtysystem-cameradrop*. This is where images will be uploaded to for processing. 

Software Setup
===============

The following steps should be carried out on your development machine which has Visual Studio 2015 installed:

1. Create a new folder to store the sample projects you'll soon download. Call this something short, e.g. c:\iotproject
2. Use the Command Prompt to navigate to the new folder where you want the project: `cd c:\iotproject` 
3. Run the git clone command: `git clone https://github.com/ms-iot/securitysystem.git` to download a copy of the basic UWP application which takes photos and uploads them to Azure Blob Storage.
4. Change directory to the project root folder: `cd securitysystem`
5. Next, get the submodules for the USB camera and the PIR sensor by running the following commands: `git submodule init` followed by `git submodule update`
6. Open the SecuritySystemUWP.sln solution file, in the SecuritySystemUWP folder, using Visual Studio 2015.

7. From the tool bar, choose "Remote Machine" as the target to deploy your application to.

8. ![Choosing the right target setting](images/remotemachine.png "Choosing the right target setting")

9. From the pop-up dialog, you should see the name and IP of your device has been autodetected (*minwinpc -- 192.168.1.5* in this case), __click 'select'__  to set it as the target for deployment device. *If your device is not shown, you can manually enter the IP address being sure to use the default authentication mode (i.e. Universal - unencrypted). The chances are however that if you don't see this appearing under "Auto Detected" that your device 1) is not functioning correctly - ensuring it has working network connectivity would be first thing to check, 2) something else! *

10. ![Remote Machine dialog](images/connections1.png) ![Remote Machine dialog2](images/connections2.png)
11. You'll now need to tell Visual Studio to compile the application for the ARM platform (which a requirement to run on the RPi). __Right-click__ on the name of your solution in Solution Explorer and choose *"Configuration Manager".* 
12. Set the entire project to compile against ARM. Be sure to check "Debug" and "Deploy" against the SecuritySystemUWP application at the same time. This ensures that when you click to run the application, it is deployed afresh to the destination device and then attaches the remote debugger.
13. Right-click on the __com.microsoft.maker.SecuritySystem__ project and select "Build". This will download nuget packages and compile this project.
14. Repeat the process for the __OneDriveConnector__, __PirSensor__ and __UsbCamera__ projects. 
15. Finally repeat for the __SecuritySystemUWP__ project. *All projects should have built with out errors (some warnings about async methods might be seen - these can be ignored).*
16. You can now deploy and test the application by pressing `F5`. *The first time you deploy an applicatioln it make take some time as required framework updates are installed onto the device - Visual Studio may even display some "This is taking too long messages" - Be patient! Subsequent deployments will be much quicker.*

Configuring the Web App
=======================

1. Use your web browser to navigate to http://*yourdeviceipaddress*:8000. You will see a landing page for your application running on your device.
2. Click on the "Settings" link fron the left hand navigation menu.
3. Change the Storage type to *Azure* and enter the storage account name and key you saved from earlier. Other settings can be left either blank or at their defaults. __The web server built into the UWP app does not support secured connections so all information is received & sent in the clear - beware!__
4. Press *Save* at the bottom of the page.

![Configuring the App on the Device via it's web interface](images/appazuresettings.png)

Testing the application
=======================

1. Attach a USB webcam to the RPi, then wait a few seconds whilst Windows loads the driver. *At the time of writing, the offical RasperyPi camera is not supported on Windows 10 IoT Core :(*
1. Moving your hand in front of the PiR sensor will cause the USB camera to take a photograph.
2. The image has been created in the Pictures folder on the device in a subdirector called XXXX. You can view the new file at http://*yourdeviceipaddress*:8000/gallery.htm or by browsing to \\*yourdeviceipaddress*\c$\Data\Users\DefaultAccount\Pictures\securitysystem-cameradrop\.
3. Every 60 seconds, these images are uploaded to the Azure blob container you created earlier. The originals on the device are deleted.

Manually taking a photograph
============================

You can manually trigger a the taking of a photo by clicking on the "Action" link on the nav menu, then clicking "Take Photo". 