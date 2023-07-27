# Player Movement Copycat
Allows to copy pose, gestures and movement of your friends.

# Installation
* Install [BTKUILib](https://github.com/BTK-Development/BTKUILib)
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `ml_pmc.dll` in `Mods` folder of game
  
# Usage
Available options in BTKUILib players list upon player selection:
* **Copy movement:** starts/stops copycating of selected player.
  * Note: Selected player should be your friend, be in your view range and not obstructed by other players/world objects/props.
* **Apply position:** enables/disables position changes of selected player; `true` by default.
  * Note: Forcibly disabled in worlds that don't allow flight.
* **Apply rotation:** enables/disables rotation changes of selected player; `true` by default.
* **Copy gestures:** enables/disables gestures copy of selected player; `true` by default.
* **Apply LookAtIK:** enables/disables additional head rotation based on camera view in desktop mode; `true` by default.
* **Mirror pose:** enables/disables pose and gestures mirroring; `false` by default.
* **Mirror position:** enables/disables mirroring of position changes of selected player along 0XZ plane; `false` by default.
* **Mirror rotation:** enables/disables mirroring of rotation changes of selected player along 0XZ plane; `false` by default.

# Notes
* Some avatars can have unordinary avatar hierarchy (scaled, rotated or with offset armature/parent objects). Possible fixes are being made upon reports or own findings.
