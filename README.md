## LIV-VNyan camera sync plugin

![ezgif-82a5aa22420e84](https://github.com/user-attachments/assets/dcdc99ee-3f80-4e9f-bb56-4fcd5e13ef3b)

Alpha, at own risk, syncs your VNyan camera over to LIV mixed reality, allowing for VNyan to be your model renderer and have a fully featured model

## Installation  
* VNyan DLLs go in the VNyan\Items\Assemblies folder  
* LIV DLL goes in C:\Users\<you>\Documents\LIV\Plugins\CameraBehaviours  
* Enable plugin from LIV within VR  
* Enable plugins in VNyan settings, if not already enabled  
* Set LIV output to double your stream res (e.g. 1920x1080 = 3840x2160) and Effect: Dump + Composite.  
![image](https://github.com/user-attachments/assets/f23fd6c9-fea4-4aca-a71c-60b4bcf9a386)
* Use [Virtual Display Driver](https://github.com/VirtualDrivers/Virtual-Display-Driver) if LIV truncates the window due to your monitor being too small. This creates a virtual monitor of a size of your choosing, which you can send LIV output to  
![image](https://github.com/user-attachments/assets/6eba1d67-3951-4e32-8e40-46c33564a0e5)

## OBS Setup
* Install plugins [Source Clone](https://obsproject.com/forum/resources/source-clone.1632/) and [Advanced Mask](https://obsproject.com/forum/resources/advanced-masks.1856/)  
* Capture LIV Output using Game Capture, name it "LIV Quadrants"  
* Using the transform editor on "LIV Quadrants", crop right: 1920 top: 1080 from your LIV Output. Do not use filters for this!  
* Source clone "LIV Quadrants", Add a crop filter, crop left: 1920 bottom: 1080 put behind LIV Output. Name it "LIV Alpha Mask". Do not use a transform for this!  
* Capture VNyan Spout2 output in the usual way  
* Source clone "LIV Quadrants" again, Add a crop filter, crop right 1920 bottom: 1080. Blending mode: add. Name it "LIV Glow"  
* Source clone "LIV Quadrants" again, Add a crop filter, crop right 1920 bottom: 1080. Name it "LIV Front Layer"
* Add a filter to "LIV Front Layer": Advanced mask, use Source "LIV Alpha Mask" and filter type "greyscale"  

The end result should look something like this:  
![image](https://github.com/user-attachments/assets/112250d1-3203-4a98-a06d-a98a56ece377)

## Use in VNyan
Clicking the plugin button toggles camera sync  
### Triggers:  
```_lum_liv_setcam``` - one off sync  
```_lum_liv_enable``` - enable camera sync  
```_lum_liv_disable``` - disable camera sync  

## Troubleshooting
### Feet or bottom of screen getting clipped  
Adjust back/forward offset in LIV's camera advanced settings  
![image](https://github.com/user-attachments/assets/d5d2ff5d-146f-429b-8996-0d1c3972cbaa)


## https://twitch.tv/LumKitty
