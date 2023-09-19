# Avatar Motion Tweaker
This mod adds features for AAS animator and avatar locomotion behaviour.

![](.github/img_01.png)

# Installation
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `ml_amt.dll` in `Mods` folder of game

# Usage
Available mod's settings in `Settings - IK - Avatar Motion Tweaker`:
* **Crouch limit:** defines crouch limit; default value - `75`.
* **Prone limit:** defines prone limit; default value - `40`.
* **IK override while flying:** disables legs locomotion/autostep in fly mode; default value - `true`.
* **IK override while jumping:** disables legs locomotion/autostep in jump; default value - `true`.
* **Follow hips on IK override:** adjusts avatar position to overcome animation snapping on IK override; default value - `true`.
  * Note: Works best with animations that have root transform position (XZ) based on center of mass.
  * Note: Made for four point tracking (head, hands and hips) in mind.
* **Detect animations emote tag:** disables avatar's IK entirely if current animator state has `Emote` tag; default value - `true`.
  * Note: Created as example for [propoused game feature](https://feedback.abinteractive.net/p/disabling-vr-ik-for-emotes-via-animator-state-tag-7b80d963-053a-41c0-86ac-e3d53c61c1e2).
* **Adjusted locomotion mass center:** automatically changes IK locomotion center if avatar has toe bones; default value - `true`.
  * Note: Compatible with [DesktopVRIK](https://github.com/NotAKidOnSteam/DesktopVRIK) and [FuckToes](https://github.com/NotAKidOnSteam/FuckToes).

Available additional parameters for AAS animator:
* **`Upright`:** defines linear coefficient between current viewpoint height and avatar's viewpoint height; float, range - [0.0, 1.0].
  * Note: Can be set as local-only (not synced) if starts with `#` character.
  * Note: Shouldn't be used for transitions between poses in desktop mode. In desktop mode its value is driven by avatar animations. Use `CVR Parameter Stream` for detecting desktop/VR modes and change AAS animator transitions accordingly.
* **`GroundedRaw`:** defines instant grounding state of player instead of delayed default parameter `Grounded`; boolean.
  * Note: Can be set as local-only (not synced) if starts with `#` character.
* **`Moving`:** defines movement state of player; boolean.
  * Note: Can be set as local-only (not synced) if starts with `#` character.

Additional mod's behaviour:
* Overrides and fixes IK behaviour in 4PT mode (head, hands and hips).
* Avatars can have controlled IK crouch and prone limits. For that create `[IKLimits]` GameObject parented to avatar's root. Its local X and Y positions will be used as crouch and prone limits respectively and can be changed via animations. Values should be in range of [0;1].