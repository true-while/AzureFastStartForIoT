# Fraud Detection Scenario



<img src="https://hackster.imgix.net/uploads/cover_image/file/66861/SecurityCamera2.JPG?auto=compress%2Cformat&w=400">
<img src="https://hackster.imgix.net/uploads/cover_image/file/91527/project%20picture.png?auto=compress%2Cformat&w=400">

Scenario
========

For this project you will attach a regular USB camera to your Windows IoT Core device along with a PIR sensor (the required hardware will have been supplied to you by your instructor).
This basic system will detect movement near the camera and trigger it to take a photograph. The image will be uploaded to Azure where Cortana Analytics will analyse the photo.
 __TODO: What will it do with it?__

 Basic Setup
 -----------

* Set up your PC and RPi according to these [instructions](http://ms-iot.github.io/content/en-US/win10/SetupPCRPI.htm).
* 
* Next, wire the PIR sensor as shown in the image below. __Note: It is a counter-intuitive but the PIR sensor uses the white wire for ground and the black wire for the signal.


![Connect the following using a 10 kΩ pull-up resistor.](https://hackster.imgix.net/uploads/image/file/68626/PIR_bb.png?auto=compress%2Cformat&amp;w=680&amp;h=510&amp;fit=max "Connect the following using a 10 kΩ pull-up resistor.")



