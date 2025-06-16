## LIV-VNyan camera sync plugin

![ezgif-82a5aa22420e84](https://github.com/user-attachments/assets/dcdc99ee-3f80-4e9f-bb56-4fcd5e13ef3b)

Alpha, at own risk, syncs your VNyan camera over to LIV mixed reality, allowing for VNyan to be your model renderer and have a fully featured model

## Overview
It is important to understand how this plugin works. Normally LIV will split the VR scene into effectively three image layers, background, glow and foreground. It will then build up a composite image: Background, VRM avatar, glow layer and foreground. This setup works by replacing the VRM avatar layer with a capture from VNyan and manually doing the compositing of the layers in OBS, instead of letting LIV do it. In order to make VNyan fit correctly there are a pair of plugins, one for VNyan that sends its camera position, over to the LIV plugin which sets the camera plugin to match VNyan.

## Prerequisites
* VNyan
  * SteamVR tracking set up and working
* OBS
  * [Source Clone](https://obsproject.com/forum/resources/source-clone.1632/) plugin
  * [Advanced Mask](https://obsproject.com/forum/resources/advanced-masks.1856/) plugin 

## Installation  
* Enable plugins in VNyan settings, if not already enabled  
* Copy VNyan-LIV.dll into the VNyan\Items\Assemblies folder  
* Copy LIV_VNyan.dll into C:\Users\<you>\Documents\LIV\Plugins\CameraBehaviours  
* Enable plugin from LIV within VR (Camera 1 -> Plugin -> LIVnyan)  
* Set LIV output to double your stream res (e.g. 1920x1080 = 3840x2160) and Effect: Dump + Composite.  
![image](https://github.com/user-attachments/assets/f23fd6c9-fea4-4aca-a71c-60b4bcf9a386)
* Use [Virtual Display Driver](https://github.com/VirtualDrivers/Virtual-Display-Driver) if LIV truncates the window due to your monitor being too small. This creates a virtual monitor of a size of your choosing, which you can send LIV output to  
![image](https://github.com/user-attachments/assets/6eba1d67-3951-4e32-8e40-46c33564a0e5)

## OBS Setup
* Capture LIV Output using Game Capture, name it "LIV Quadrants"  
  * Using the transform editor on "LIV Quadrants", crop right: 1920 top: 1080 from your LIV Output. Do not use filters for this!  
* Source clone "LIV Quadrants", name it "LIV Alpha Mask"
  * Add a crop filter, crop left: 1920 bottom: 1080. Do not use a transform for this!
  * Place this *behind* the "LIV Quadrants" source so it is completely hidden  
* Capture VNyan Spout2 output in the usual way
  * place this in front of "LIV Quadrands"  
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

## Lighting (optional)
Use [Sjatar's Stylistic Screen Light plugin](https://github.com/Sjatar/StylisticScreenLight)  
Add a "Sprout Sender" filter to "LIV Front Layer" after crop/pad but before Advanced Mask. Name the source "screen"

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


## https://twitch.tv/LumKitty
