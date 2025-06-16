## LIV-VNyan camera sync plugin

![ezgif-82a5aa22420e84](https://github.com/user-attachments/assets/dcdc99ee-3f80-4e9f-bb56-4fcd5e13ef3b)

Alpha, at own risk, syncs your VNyan camera over to LIV mixed reality, allowing for VNyan to be your model renderer and have a fully featured model

## Overview
It is important to understand how this plugin works. Normally LIV will split the VR scene into effectively three image layers, background, glow and foreground. It will then build up a composite image: Background, VRM avatar, glow layer and foreground. This setup works by replacing the VRM avatar layer with a capture from VNyan and manually doing the compositing of the layers in OBS, instead of letting LIV do it. In order to make VNyan fit correctly there are a pair of plugins, one for VNyan that sends its camera position, over to the LIV plugin which sets the camera plugin to match VNyan.

## Prerequisites
* VNyan
  * SteamVR tracking set up and working
  * Spout2 output configured and working
* OBS
  * [Spout2](https://github.com/Off-World-Live/obs-spout2-plugin) plugin
  * [Source Clone](https://obsproject.com/forum/resources/source-clone.1632/) plugin
  * [Advanced Mask](https://obsproject.com/forum/resources/advanced-masks.1856/) plugin 

## Installation  
* Enable plugins in VNyan settings, if not already enabled  
* Copy VNyan-LIV.dll into the VNyan\Items\Assemblies folder  
* Copy LIV_VNyan.dll into C:\Users\<you>\Documents\LIV\Plugins\CameraBehaviours

## LIV Setup
* Start Virtual Cameras and Avatars
* Set capture to "Manual"
  * Target: your game
  * Effect: Dump + Composite  
  ![image](https://github.com/user-attachments/assets/f23fd6c9-fea4-4aca-a71c-60b4bcf9a386)  
* Set LIV output to double your stream res (e.g. 1920x1080 = 3840x2160)
  ![image](https://github.com/user-attachments/assets/6eba1d67-3951-4e32-8e40-46c33564a0e5)
* Put your VR headset on, open LIV from the circle on the floor and enable avatars
* Go to: Camera 1 -> Plugin -> LIVnyan to enable the plugin
* Do one of the following:
  * (Better performance) Disable rendering all parts of your avatar within LIV and disable tracking, or
  * (Better visiblity) Load a VRM version of your avatar, and enable whatever trackers you actually have

## OBS Setup
* Capture LIV Output using Game Capture, name it "LIV Quadrants"  
  * Using the transform editor on "LIV Quadrants", crop right: 1920 top: 1080 from your LIV Output. Do not use filters for this!  
* Source clone "LIV Quadrants", name it "LIV Alpha Mask"
  * Add a crop filter, crop left: 1920 bottom: 1080. Do not use a transform for this!
  * Place this *behind* the "LIV Quadrants" source so it is completely hidden  
* Capture VNyan Spout2 output in the usual way
  * place this in front of "LIV Quadrants"  
* Source clone "LIV Quadrants" again, name it "LIV Glow".
  * Add a crop filter, crop right 1920 bottom: 1080.
  * Right click it -> Blending Mode -> Add
  * place this in front of your VNyan Spout capture  
* Source clone "LIV Quadrants" again, name it "LIV Front Layer".
  * Add a crop filter, crop right 1920 bottom: 1080.
  * Place this in front of "LIV Glow"
  * Add an "Advanced Mask" filter
    * Source: "LIV Alpha Mask"
    * Filter type: "greyscale"
  ![image](https://github.com/user-attachments/assets/d530679a-1a00-4619-bfac-eb09ddfb9e44)

  The end result should look something like this:  
  ![image](https://github.com/user-attachments/assets/112250d1-3203-4a98-a06d-a98a56ece377)

## VNyan Setup
* Configure SteamVR and calibrate your trackers in the usual way
* Ensure the plugin is active by one of the following methods
  * Click the button in the VNyan plugins screen to toggle enable/disable
  * Call the ```_lum_liv_enable``` trigger
  * Edit LIVnyan.cfg to set ActiveOnStart = true and restart VNyan
 
## Notes on calibration
VNyan tracker callibration will need some experimentation to find the method that works best for you. For me I find it best to stand about 30cm behind the feet markers (in Beat Saber) and then run VNyan's calibration. Please share any tips & tricks you find for this.  
Many VRoid models have their arms too short. The closer your model matches your IRL height and proportions the less likely you are to have weapons fly away from your hand when your arms are outstretched. Elbow IK should also work better
For games that don't show objects/weapons in your hand, these calibration issues will be less noticible  
Unfortunately, VMC output from LIV was removed. Using VMC output would completely solve this problem. Please ask the LIV developers to re-instate VMC output support :D

## Final Checks before going live 
* LIV manual target and effect set correctly (LIV does not remember these settings)
* Check your OBS scene and ensure that your model appears in the correct place
* Drag the camera around in the VNyan window, and check that LIV's output follows it
* Are foreground objects and glowing objects correctly passing in front of your model
* Are your feet being chopped off (See the troubleshooting section for this)
* Have any physics on your model glitched while you were doing all this? (reload avatar if so)

## Use in VNyan
Clicking the plugin button toggles camera sync  
### Triggers:  
```_lum_liv_enable``` - enable camera sync  
```_lum_liv_disable``` - disable camera sync  

## Configuration
Settings are stored in LIVnyan.cfg inside your VNyan profile directory (default %APPDATA%\..\LocalLow\Suvidriel\VNyan)  
```ActiveOnStart``` - Camera sync will start as soon as VNyan loads  
```LogEnabled``` - The VNyan plugin will log to player.log in the main VNyan profile directory. The LIV plugin will log to
%USERPROFILE%\Documents\LIV\Plugins\CameraBehaviours\LIVnyan.log  
```LogSpam``` - Both plugins will log sent/recieved camera position, rotation & FOV, plus settings info every single frame.  
These logs will get very big very quickly. Only enable this for troubleshooting!  

## Troubleshooting
### Feet or bottom of screen getting clipped  
Adjust back/forward offset in LIV's camera advanced settings  
![image](https://github.com/user-attachments/assets/d5d2ff5d-146f-429b-8996-0d1c3972cbaa)
### LIV output window is cropped
If your monitor resolution is too small, LIV will crop the output window to your actual size. If you are streaming at 1080p and do not have a 4K monitor this will affect you.  
Use [Virtual Display Driver](https://github.com/VirtualDrivers/Virtual-Display-Driver) to create a fake 3840x2160 display. As an added bonus this hides the LIV output window from your actual monitor which makes it significantly less annoying!
### Irritating green dot moving around the edge of your VR view
Another LIV feature. It's suppossed to tell you where your camera is currently. Apparently the only way to disable it is to set camera 1 to selfie and then you have the option to disable it, then you can set it back to plugin. This setting does not persist between sessions. Please report this bug to the LIV developers!

## Lighting (optional)
Use [Sjatar's Stylistic Screen Light plugin](https://github.com/Sjatar/StylisticScreenLight)  
Add a "Sprout Sender" filter to "LIV Front Layer" after crop/pad but before Advanced Mask. Name the source "screen"
![image](https://github.com/user-attachments/assets/8909d36c-0549-4202-b862-32b6911a4417)

## Shameless plug
### https://twitch.tv/LumKitty
If you find this plugin useful, please consider sending a follow or a raid my way. If you somehow make millions using it, consider sharing some of that with me :3
