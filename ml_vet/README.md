# Vive Eye Tracking
This mod complements functionality of in-game SRanipal face tracking module and makes it work additionaly as eye tracking module.

# Why?
* Game has unfinished/unused and forcibly disabled eye tracking code that actually uses SRanipal API instead of TobiiXR.
* Implemented native TobiiXR eye tracking is very unreliable. It freezes main thread with 66% chance at game launch and has limited detection of eyes openness.

# Benefits?
* SRanipal API supports ranged eyes openness detection, unlike TobiiXR. You can finally smirk now.
* There is no main thread freeze at game launch.
* Semi-ready for future game update with separated eyes blinking.

# Installation
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `ViveEyeTracking.dll` in `Mods` folder of game
  
# Usage
Available mod's settings in `Settings - Implementation - Vive Eye Tracking`:
* **Enable eye tracking:** eye tracking state; `false` by default.
  * Note: If Vive face tracking is already running, enabling requires Vive face tracking restart.
