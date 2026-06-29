# QuickStart Instructions for using LIVnyan with OnAirTap instead of LIV

## Prerequisites
* VNyan
  * SteamVR tracking configured and working
  * Spout2 output configured and working
  * (Recommended) a .VSFAvatar model
  * (Optional) My [Extras Plugin](https://github.com/LumKitty/LumsExtras/) to prevent accidentally resizing VNyan while in VR
* OBS
  * [Spout2](https://github.com/Off-World-Live/obs-spout2-plugin) plugin
  * [Advanced Mask](https://obsproject.com/forum/resources/advanced-masks.1856/) plugin
* OnAirTap
  * Understanding of how to mod your chosen game (BepInEx or BSIPA)
* General
  * A sufficiently powerful PC
  * Understanding of OBS filters
  * Understanding of the Windows filesystem, DLL files, JSON files etc.

## Installation  
* Download the latest version from the [Releases Page](https://github.com/LumKitty/LIVnyan/releases) - needs to be at least 1.3a2
* Enable plugins in VNyan settings, if not already enabled  
* Copy VNyan-LIV.dll into the VNyan\Items\Assemblies folder  
* Copy LIV_VNyan.dll into C:\Users\\\<you>\Documents\LIV\Plugins\CameraBehaviours

## OnAirTap setup - BepInEx
* Download the latest version of [OnAirTap](https://github.com/LumKitty/LIVnyan/releases/tag/OnAirTap-binary)
* Install the latest version of [BepInEx](https://github.com/bepinex/bepinex) following its instructions
* Install the mod by unpacking the OpenBrush build. If you are attempting to use this with a different game you will need to use a different directory for OAT_KlakSpout.dll instead of OpenBrush_Data. Typically it will be GameName_Data
* Start the game once and quit
* Edit <GAMEDIR>\BepInEx\config\OnAirTap.cfg
* Recommended initial settings:
  ```
  [ClipPlanes]
  (leave everything as default)
  
  [OAT_MMF_Data]
  ReadWindowResolution = true
  ReadClipPlaneLocation = true
  ProtocolMinorVersion = 1

  [RenderPasses]
  RenderBackground = true
  RenderForeground = true
  RenderOptimised = true
  SendBackground = true
  SendForeground = true
  SendOptimised = true
  BlankSendersOnRenderDispose = true
  (leave everything else as default)
  ```
* Start VNyan and press the LIVnyan button to enable sending data. If successful your model should appear smaller than usual, due to the necessary camera changes
* Start your game

## OBS Setup
* Create a Spout2 capture named "OnAirTap-BG". Choose the SpoutSender "OnAirTap Background", with Composite Mode set to Opaque
* Immediately above this, create a Spout2 capture for VNyan with Composite Mode set to default
* Create a Spout2 capture named "OnAirTap-FG". Choose the SpoutSender "OnAirTap Foreground", with Composite Mode set to Opague
* Right click OnAirTap-FG -> Blending Mode -> Add
* Right click OnAirTap-FG -> Filters
* Hit the + button and add an effect filter "Advanced Mask". Set the following settings:
  Mask Effect: Alpha Mask
  Mask Type: Source
  Source: VNyan
  Scaling: Manual
  Scale By: Percent (preserve aspect ratio)
  Scale: 100%
  Filter On: Alpha Channel
  (all other settings should be default)
* Create a Spout2 capture named "OnAirTap-FG2". Choose the SpoutSender "OnAirTap Foreground", with Composite Mode set to Default
* (Optional) group these up.

## OnAirTap setup - Beat Saber
* Setup is the same as for BepInEx except that the config file is now <GAMEDIR>\UserData\OnAirTap.json and it is not split into individual sections
* OBS setup is the same except instead of OnAirTap-FG2 create OnAirTap-OP. Choose the SpoutSender "OnAirTap Optimised", with composite mode set to default

For more efficient setups, and Linux support, see the main OnAirTap readme

Continue following the main instructions at [LIVnyan - VNyan Setup](https://github.com/LumKitty/LIVnyan#vnyan-setup)
