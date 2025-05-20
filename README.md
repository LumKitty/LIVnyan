## LIV-VNyan camera sync plugin

![ezgif-82a5aa22420e84](https://github.com/user-attachments/assets/dcdc99ee-3f80-4e9f-bb56-4fcd5e13ef3b)

Alpha, at own risk, syncs your VNyan camera over to LIV mixed reality, allowing for VNyan to be your model renderer and have a fully featured model

VNyan DLLs go in the VNyan\Items\Assemblies folder  
LIV DLL goes in C:\Users\<you>\Documents\LIV\Plugins\CameraBehaviours  
Enable plugin from LIV within VR  
Set LIV output to double your stream res (e.g. 1920x1080 = 3840x2160) and Effect: Dump + Composite.  
Use https://github.com/VirtualDrivers/Virtual-Display-Driver if LIV truncates the window due to your monitor being too small

## OBS Setup
Install plugins Source Clone and Advanced Mask  
Capture LIV Output  
Using the transform editor, crop right: 1920 top: 1080 from your LIV Output  
Source clone LIV Output, crop left: 1920 bottom: 1080 put behind LIV Output. Name it Alpha Mask  
Capture VNyan sprout as usual  
Source clone LIV Output, crop right 1920 top: 1080. Blending mode: add. Name it Glow  
Source clone LIV Output, crop right 1920 top: 1080. Filter: Advanced mask, use Source "Alpha Mask" and filter greyscale  

## Use in VNyan
Clicking the plugin button toggle camera sync  
triggers:
```_lum_liv_setcam``` - one off sync
```_lum_liv_enable``` - enable camera sync
```_lum_liv_disable``` - disable camera sync

## https://twitch.tv/LumKitty
