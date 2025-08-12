## LIVNyan - A camera sync plugin for VNyan and LIV VR
Syncs your VNyan camera over to LIV mixed reality  
Allows VNyan to be your VTuber renderer allowing for high quality models and full redeem support - no more VRMs!  

![A catgirl VTuber plays Beat Saber](https://github.com/LumKitty/LIVnyan/blob/master/LIVnyan_demo.gif?raw=true)  
Please read these instructions carefully before trying to set this up!


## Overview
It is important to understand how this plugin works. Normally LIV will split the VR scene into effectively three image layers, background, glow and foreground. It will then build up a composite image: Background, VRM avatar, glow layer and foreground. This setup works by replacing the VRM avatar layer with a capture from VNyan and manually doing the compositing of the layers in OBS, instead of letting LIV do it. In order to make VNyan fit correctly there are a pair of plugins, one for VNyan that sends its camera position, over to the LIV plugin which sets the camera plugin to match VNyan.

## Prerequisites
* VNyan
  * SteamVR tracking configured and working
  * Spout2 output configured and working
  * (Recommended) a .VSFAvatar model [^1]
  * (Optional) My [Extras Plugin](https://github.com/LumKitty/LumsExtras/) to prevent accidentally resizing VNyan while in VR
* OBS
  * [Spout2](https://github.com/Off-World-Live/obs-spout2-plugin) plugin
  * [Source Clone](https://obsproject.com/forum/resources/source-clone.1632/) plugin
  * [Advanced Mask](https://obsproject.com/forum/resources/advanced-masks.1856/) plugin
* LIV VR
  * Mixed Reality Avatar mode configured and working
  * (Optional) A VRM file with the same rigging as your VSFAvatar
* General
  * A sufficiently powerful PC
  * Understanding of OBS filters
  * Understanding of the Windows filesystem, DLL files, JSON files etc.

## Installation  
* Download the latest version from the [Releases Page](https://github.com/LumKitty/LIVnyan/releases)
* Enable plugins in VNyan settings, if not already enabled  
* Copy VNyan-LIV.dll into the VNyan\Items\Assemblies folder  
* Copy LIV_VNyan.dll into C:\Users\\\<you>\Documents\LIV\Plugins\CameraBehaviours

## LIV Setup
* Start Virtual Cameras and Avatars
* Set capture to "Auto"
  * Target resolution: Your OBS canvas size, or maybe smaller if you stream with large borders
  ![image](https://github.com/user-attachments/assets/9c94ebea-2e0b-48eb-86c9-188a8369821e)
* Set capture to "Manual"
  * Target: your game
  * Effect: Dump + Composite  
  ![image](https://github.com/user-attachments/assets/f23fd6c9-fea4-4aca-a71c-60b4bcf9a386)  
* Set LIV output to double your target resolution (e.g. 1920x1080 = 3840x2160)
  ![image](https://github.com/user-attachments/assets/6eba1d67-3951-4e32-8e40-46c33564a0e5)
* Put your VR headset on, open LIV from the circle on the floor and enable avatars
* Go to: Camera 1 -> Plugin -> LIVnyan to enable the plugin
* Do one of the following:
  * (Better performance) Disable rendering all parts of your avatar within LIV and disable tracking, or
  * (Better visiblity) Load a VRM version of your avatar, and enable whatever trackers you actually have
  * (Best option, but paid) Disable rendering & tracking in LIV, pop out the OBS preview to a separate window and then use XSOverlay (or similar) to display that preview within VR

## OBS Setup
Note: The numbers in this section assume your target resolution is 1920x1080. If it is not then subsitue your actual target resolution as appropriate
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
* Ensure that VNyan's window size is the same size as all your LIV sources  
* Configure SteamVR and calibrate your trackers in the usual way
* Ensure the plugin is active by one of the following methods
  * Click the button in the VNyan plugins screen to toggle enable/disable
  * Call the ```_lum_liv_enable``` trigger
  * Edit LIVnyan.cfg to set ActiveOnStart = true and restart VNyan
 
## Notes on calibration
VNyan tracker callibration will need some experimentation to find the method that works best for you. For me I find it best to stand about 30cm behind the feet markers (in Beat Saber) and then run VNyan's calibration. Please share any tips & tricks you find for this.  
Many VRoid models have their arms too short. The closer your model matches your IRL height and proportions the less likely you are to have weapons fly away from your hand when your arms are outstretched. Elbow IK should also work better.  
For games that don't show objects/weapons in your hand, these calibration issues will be less noticible  

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

## Final Checks before going live 
* LIV manual target and effect set correctly (LIV does not remember these settings)
* Check your OBS scene and ensure that your model appears in the correct place
* Drag the camera around in the VNyan window, and check that LIV's output follows it
* Are foreground objects and glowing objects correctly passing in front of your model
* Are your feet being chopped off (See the troubleshooting section for this)
* Have any physics on your model glitched while you were doing all this? (reload avatar if so)

## Lum's recommendations
These are completely optional, but are how I do things
* Have an "EnableVR" websocket trigger that enables SteamVR and LipSync, disables ARKit & LeapMotion. Also have a "DisableVR" websocket that does the opposite. Put these on a toggle button on your stream deck.
* If you have physics, especially skirts, and will be taking stream breaks: have a scene change button for your VR scene that waits for ~4 seconds, then reloads your avatar, waits another second and then switches scene. This will minimise any issues with skirt clipping caused by pretzeling, giving you time to get to a neutral position in your VR space before forcing a physics reset
* If you use VoiceMeeter, have both your regular microphone and your VR headset microphone routed to the same virtual audio cable. Connect OBS, discord etc. to this virtual cable, then have a stream deck button that mutes one and unmutes the other, and vice versa
* Also in VoiceMeeter, have a virtual audio cable for discord comms and/or redeems, have this routed to both your regular headphones and your VR headset
* VoiceMeeter again: Have a "Rescan audio" button that runs ```Voicemeeter.exe -r``` Since the Index headset is generally not connected until you launch SteamVR, VoiceMeeter will not pick it up as a valid sound target on launch, but forcing a rescan like this fixes that without having to manually kill and re-open it
* If your are playing a rhythm game and are routing your audio through VoiceMeeter, carefully check your audio calibration. For me the lag created by voicemeeter is 120ms, which is quite significant
* Configure VNyan to switch to 1920x1080 on launch, then use my [Extras Plugin](https://github.com/LumKitty/LumsExtras/) to lock VNyan's window size. Preventing accidental resizing errors when you're trying to do an emergency recalibrate
* Use XSOverlay or similar to show your OBS preview window within VR, and check it frequently, especially if you have physics on your model
* Take full advantage of VNyan's features. Go wild with redeems, Poiyomi shaders, Magica cloth. You put all the effort in to set this up, so make use of it!
* Follow LumKitty on https://twitch.tv/LumKitty :3

## Troubleshooting
### Feet or bottom of screen getting clipped  
Adjust back/forward offset in LIV's camera advanced settings  
![image](https://github.com/user-attachments/assets/d5d2ff5d-146f-429b-8996-0d1c3972cbaa)
### LIV output window is cropped
If your monitor resolution is too small, LIV will crop the output window to your actual size. If you are streaming at 1080p and do not have a 4K monitor this will affect you.  
Use [Virtual Display Driver](https://github.com/VirtualDrivers/Virtual-Display-Driver) to create a fake 3840x2160 display. As an added bonus this hides the LIV output window from your actual monitor which makes it significantly less annoying (although having a fake monitor is annoying in different ways, so remember to disable it outside of VR streaming)
WARNING: If you are using Virtual Display Driver. Ensure that you enable the display before you start SteamVR. Otherwise you will hit a bug SteamVR when trying to interact with your desktop that causes the mouse pointer to be massively offset from where you point your controller. Only fix at this point is to restart SteamVR. Guaranteed this will bite you in the arse mid-stream, so be careful!  
### Irritating green dot moving around the edge of your VR view
Another LIV feature. It's suppossed to tell you where your camera is currently. Apparently the only way to disable it is to set camera 1 to selfie and then you have the option to disable it, then you can set it back to plugin. This setting does not persist between sessions. Unfortunately this is not something I can fix.  
### in-game objects move before your VTuber
Increase the Camera latency in LIV until it lines up

## Lighting (optional)
Use [Sjatar's Stylistic Screen Light plugin](https://github.com/Sjatar/StylisticScreenLight)  
Add a "Sprout Sender" filter to "LIV Front Layer" after crop/pad but before Advanced Mask. Name the source "screen"
![image](https://github.com/user-attachments/assets/8909d36c-0549-4202-b862-32b6911a4417)

## Support
Either open an issue here (preferred), or ask in the LIVnyan thread on [Suvi's Discord](https://discord.com/channels/714814460010823690/1373846127975207002)

## Shameless plug
### https://twitch.tv/LumKitty
If you find this plugin useful, please consider sending a follow or a raid my way. If you somehow make millions using it, consider sharing some of that with me :3

[^1]: While you can use a VRM model with LIVnyan. Given that LIV natively supports VRM files it is probably not worth setting up LIVnyan, unless you really need VNyan redeems to work!
