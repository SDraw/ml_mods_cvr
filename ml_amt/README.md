# Avatar Motion Tweaker
This mod adds features for AAS animator and avatar locomotion behaviour.

![](.github/img_01.png)

# Installation
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `ml_amt.dll` in `Mods` folder of game

# Usage
Available mod's settings in `Settings - IK - Avatar Motion Tweaker`:
* **IK override while crouching:** disables legs locomotion/autostep upon HMD reaching `Crouch limit`; default value - `true`.
* **Crouch limit:** defines crouch limit; default value - `65`.
  * Note: Can be overrided by avatar. For this avatar has to have child gameobject with name `CrouchLimit`, its Y-axis location will be used as limit, should be in range [0.0, 1.0].
* **IK override while proning:** disables legs locomotion/autostep upon HMD reaching `Prone limit`; default value - `true`.
* **Prone limit:** defines prone limit; default value - `30`.
  * Note: Can be overrided by avatar. For this avatar has to have child gameobject with name `ProneLimit`, its Y-axis location will be used as limit, should be in range [0.0, 1.0].
* **IK override while flying:** disables legs locomotion/autostep in fly mode; default value - `true`.
* **IK override while jumping:** disables legs locomotion/autostep in jump; default value - `true`.
* **Follow hips on IK override:** adjusts avatar position to overcome animation snapping on IK override; default value - `true`.
  * Note: Works best with animations that have root transform position (XZ) based on center of mass.
  * Note: Made for four point tracking (head, hands, hips) in mind.
* **Pose transitions:** allows regular avatars animator to transit in crouch/prone states; default value - `true`.
  * Note: Avatar is considered as regular if its AAS animator doesn't have `Upright` parameter.
* **Adjusted pose movement speed:** scales movement speed upon crouching/proning; default value - `true`.
* **Detect animations emote tag:** disables avatar's IK entirely if current animator state has `Emote` tag; default value - `true`.
  * Note: Created as example for [propoused game feature](https://feedback.abinteractive.net/p/disabling-vr-ik-for-emotes-via-animator-state-tag-7b80d963-053a-41c0-86ac-e3d53c61c1e2).
* **Adjusted locomotion mass center:** automatically changes IK locomotion center if avatar has toe bones; default value - `true`.
  * Note: Compatible with [DesktopVRIK](https://github.com/NotAKidOnSteam/DesktopVRIK) and [FuckToes](https://github.com/NotAKidOnSteam/FuckToes).
* **Scaled locomotion steps:** scales VRIK locomotion steps according to avatar height and scale; default value - `true`.
#### Fixes/overhauls options
* **Scaled locomotion jump:** scales locomotion jump according to relation between your player settings height and current avatar height (includes avatar scale); default value - `false`.
  * Note: Disabled in worlds that don't allow flight.
* **Alternative avatar collider scale:** applies slightly different approach to avatar collider size change; default value - `true`.
* **Fix animation overrides (chairs, etc.):** fixes animations overriding for avatars with AAS; default value - `true`.
  * Note: This option is made to address [broken animator in chairs and combat worlds issue](https://feedback.abinteractive.net/p/gestures-getting-stuck-locally-upon-entering-vehicles-chairs).

Available additional parameters for AAS animator:
* **`Upright`:** defines linear coefficient between current viewpoint height and avatar's viewpoint height; float, range - [0.0, 1.0].
  * Note: Can be set as local-only (not synced) if starts with `#` character.
  * Note: Defining this parameter in AAS animator will consider avatar as compatible with mod.
  * Note: Can't be used for transitions between poses in desktop mode. In desktop mode its value is driven by avatar animations. Use `CVR Parameter Stream` for detecting desktop/VR modes and change AAS animator transitions accordingly.
* **`GroundedRaw`:** defines instant grounding state of player instead of delayed default parameter `Grounded`; boolean.
  * Note: Can be set as local-only (not synced) if starts with `#` character.
* **`Moving`:** defines movement state of player; boolean.
  * Note: Can be set as local-only (not synced) if starts with `#` character.

Additional mod's behaviour:
* Overrides and fixes IK behaviour in 4PT mode (head, hands, hips). Be sure to disable legs and knees tracking in `Settings - IK tab`.

https://user-images.githubusercontent.com/4295751/233663668-adf5eaa6-8195-4fd2-90d5-78d61fe3fe58.mp4

https://user-images.githubusercontent.com/4295751/233663726-80a05323-aed2-41fb-9b00-7d5024ebf247.mp4
