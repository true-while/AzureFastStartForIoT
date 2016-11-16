# Fraud Detection Scenario



<img src="https://hackster.imgix.net/uploads/cover_image/file/66861/SecurityCamera2.JPG?auto=compress%2Cformat&w=400">
<img src="https://hackster.imgix.net/uploads/cover_image/file/91527/project%20picture.png?auto=compress%2Cformat&w=400">

Scenario
========

For this project you will attach a regular USB camera to your Windows IoT Core device along with a PIR sensor (the required hardware will have been supplied to you by your instructor).
This basic system will detect movement near the camera and trigger it to take a photograph. The image will be uploaded to Azure where Cortana Analytics will analyse the photo.
 __TODO: What will it do with it?__

 Basic Hardware Setup
 -----------

* Set up your PC and RPi according to these [instructions](http://ms-iot.github.io/content/en-US/win10/SetupPCRPI.htm).
* Next, wire the PIR sensor as shown in the image below being sure to use 10 kΩ pull-up resistor.

![Connect the following using a 10 kΩ pull-up resistor.](https://hackster.imgix.net/uploads/image/file/68626/PIR_bb.png?auto=compress%2Cformat&amp;w=680&amp;h=510&amp;fit=max "Connect the following using a 10 kΩ pull-up resistor.")

 Software Setup
 --------------

* Use Command Prompt to navigate to the folder where you want the project: `cd` 
* Run the git clone command: `git clone https://github.com/ms-iot/securitysystem.git`
* Change directory to the project root folder: `cd securitysystem`
* Next, get the submodules for the USB camera and the PIR sensor by running the following commands: `git submodule init` followed by `git submodule update`
* Open the SecuritySystemUWP.sln solution file, in the SecuritySystemUWP folder, using Visual Studio 2015. 
* On the top menu of Visual Studio, select Debug and ARM if you are using a Raspberry Pi. If you're using an MBM, select Debug and x86.
* Next click Build -> Clean Solution. Wait for the solution to clean successfully. 
* Select Remote Machine. You will have to enter your Remote Machine IP address and use "Universal (Unencrypted Protocol)" for Authentication Mode. *Use WindowsIoTCoreWatcher to find your IP address*.
* You can now run the code!

Running the Web App
-------------------

* To view your images, use your web browser to navigate to http://youripaddress:8000
* Select Gallery from the left panel. 
* You can now view the images.