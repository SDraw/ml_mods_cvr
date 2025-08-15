# Vive Eye Tracking
This mod complements functionality of in-game SRanipal face tracking module and makes it work additionaly as eye tracking module.

# Why?
* Game has unfinished/unused and forcibly disabled eye tracking code that actually uses SRanipal API instead of TobiiXR.
* Implemented native TobiiXR eye tracking is very unreliable. It freezes main thread with 66% chance at game launch and has limited detection of eyes openness.

# Benefits?
* SRanipal API supports ranged eyes openness detection, unlike TobiiXR. You can finally squint now.
* There is no main thread freeze at game launch.
* Ready for future game update with separated eyes blinking (currently only nightly).

# Installation
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `ViveEyeTracking.dll` in `Mods` folder of game
  
# Usage
Available mod's settings in `Settings - Implementation - Vive Eye Tracking`:
* **Enable eye tracking:** eye tracking state; `true` by default.
  * Note: If Vive face tracking is already running, enabling requires Vive face tracking restart.
* **Gaze smoothing:** smoothing of gaze point; 5 by default.
* **Debug gizmos:** show debug lines of SRanipal gazes directions and calculated gaze point; `false` by default.
  
# Notes
* Made primarily for Vive Pro Eye and SRanipal tracking.
