# Game Main Fixes
This mod fixes some issues that are present in game

# Installation
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `ml_gmf.dll` in `Mods` folder of game

# Implemented fixes
* Fix of broken `Vive Advanced Controls` game input option
  * Additional feature: Disables gestures when moving with Vive controllers
* Fix of post-processing layer volume trigger for VR camera ([feedback post](https://feedback.abinteractive.net/p/2023r171ex1-post-process-volume-effects-are-applied-based-on-playspace-center-instead-of-camera-s-in-vr-mode))
* Fix of shared `AnimatorOverrideController` between same avatars that leads to broken avatar animator
* Fix of animation replacement (chairs, etc.) that leads to broken avatar animator ([feedback post](https://feedback.abinteractive.net/p/gestures-getting-stuck-locally-upon-entering-vehicles-chairs))

# Notes
Some of fixes will be implemented natively in game after 2023r172ex3
