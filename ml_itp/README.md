# Index Trackpad Params
This mod allows you to use Index controllers trackpad input as avatar parameters.

# Installation
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Install [latest VRBinding](https://github.com/dakyneko/DakyModsCVR)
* Get [latest release DLL](../../../releases/latest):
  * Put `IndexTrackpadParams.dll` in `Mods` folder of game

# Usage
## SteamVR controller bindings
Assign new input actions for left and right controllers in SteamVR game's bindings:
* `index(left/right)trackpadtouch` - as trackpad touch
* `index(left/right)trackpadaxis` - as trackpad position
* `index(lef/right)trackpadforce` - as trackpad force sensor
## Avatar parameters
Available parameters for avatar's animator:
* `IndexLeftTrackpadTouch`, `IndexRightTrackpadTouch`: true/false as boolean; 0 or 1 as float; 0 or 1 as integer
* `IndexLeftTrackpadForce`, `IndexRighTrackpadForce`: true/false as boolean; in [0;1] range as float; 0 or 1 as integer
* `IndexLeftTrackpadAxis-x`, `IndexRightTrackpadAxis-x`: in [-1;1] range as float; -1, 0, 1 as integer
  * **Note:** -1 corresponds to left edge of trackpad, 0 - center, 1 - right edge
* `IndexLeftTrackpadAxis-y`, `IndexRightTrackpadAxis-y`: in [-1;1] range as float; -1, 0, 1 as integer
  * **Note:** -1 corresponds to bottom edge of trackpad, 0 - center, 1 - top edge
  
# Notes
Even that mod was made primarily for Index controllers, there are no restrictions of using new input actions for other type of controllers or controls.
