# Avatar Motion Tweaker
This mod adds features for AAS animator and avatar locomotion behaviour.

![](.github/img_01.png)

# Installation
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `ml_amt.dll` in `Mods` folder of game

# Usage
Available mod's settings in `Settings - Implementation - Avatar Motion Tweaker`:
* **IK override while crouching:** disables legs locomotion/autostep upon HMD reaching `Crouch limit`; default value - `true`.
* **Crouch limit:** defines crouch limit; default value - `65`.
  * Note: Can be overrided by avatar. For this avatar has to have child gameobject with name `CrouchLimit`, its Y-axis location will be used as limit, should be in range [0.0, 1.0].
* **IK override while proning:** disables legs locomotion/autostep upon HMD reaching `Prone limit`; default value - `true`.
* **Prone limit:** defines prone limit; default value - `30`.
  * Note: Can be overrided by avatar. For this avatar has to have child gameobject with name `ProneLimit`, its Y-axis location will be used as limit, should be in range [0.0, 1.0].
* **IK override while flying:** disables legs locomotion/autostep in fly mode; default value - `true`.
* **Pose transitions:** allows regular avatars animator to transit in crouch/prone states; default value - `true`.
  * Note: Avatar is considered as regular if its AAS animator doesn't have `Upright` parameter.
* **Adjusted pose movement speed:** scales movement speed upon crouching/proning; default value - `true`.

Available additional parameters for AAS animator:
* **`Upright`:** defines linear coefficient between current viewpoint height and avatar's viewpoint height; float, range - [0.0, 1.0].
  * Note: Can be set as local-only (not synced) if starts with `#` character.
  * Note: Defining this parameter in AAS animator will consider avatar as compatible with mod.
  * Note: Can't be used for transitions between poses in desktop mode. In desktop mode its value is driven by avatar animations. Use `CVR Parameter Stream` for detecting desktop/VR modes and change AAS animator transitions accordingly.
* **`GroundedRaw`:** defines instant grounding state of player instead of delayed default parameter `Grounded`.
  * Note: Can be set as local-only (not synced) if starts with `#` character.

Additional avatars tweaks:
* If avatar has child object with name `LocomotionOffset` its local position will be used for offsetting VRIK locomotion mass center.
