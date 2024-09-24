# Avatar Motion Tweaker
This mod adds features for AAS animator and avatar locomotion behaviour.

![](.github/img_01.png)

# Installation
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `AvatarMotionTweaker.dll` in `Mods` folder of game

# Usage
Available mod's settings in `Settings - IK - Avatar Motion Tweaker`:
* **Crouch limit:** defines crouch limit; default value - `75`.
* **Prone limit:** defines prone limit; default value - `40`.
* **IK override while flying:** disables legs locomotion/autostep in fly mode; default value - `true`.
* **IK override while jumping:** disables legs locomotion/autostep in jump; default value - `true`.
* **Adjusted locomotion mass center:** automatically changes IK locomotion center if avatar has toe bones; default value - `true`.
  * Note: Compatible with [DesktopVRIK](https://github.com/NotAKidOnSteam/DesktopVRIK) and [FuckToes](https://github.com/NotAKidOnSteam/FuckToes).

Available additional parameters for AAS animator:
* **`Moving`:** defines movement state of player; boolean.
* **`MovementSpeed`:** length of vector made of default `MovementX` and `MovementY` parameters
* **`Velocity`:** current player's movement velocity in space

Parameters can be set as local-only (not synced) if start with `#` character.

Additional mod's behaviour:
* Avatars can have controlled IK crouch and prone limits. For that create `[IKLimits]` GameObject parented to avatar's root. Its local X and Y positions will be used as crouch and prone limits respectively and can be changed via animations. Values should be in range of [0;1].