# Avatar Motion Tweaker
This mod adds `Upright` parameter for usage in AAS animator and allows disabling legs autostep upon reaching specific `Upright` value.

![](.github/img_01.png)

# Installation
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `ml_amt.dll` in `Mods` folder of game

# Usage
Available mod's settings in `Settings - Implementation - Avatar Motion Tweaker`:
* **Legs locomotion upright limit:** defines upright limit of legs autostep. If HMD tracking goes below set limit, legs autostep is disabled. Default value - 65.
  * Limit can be overrided by avatar. For this avatar has to have child gameobject with name `CrouchLimit` and its Y-axis location will be used as limit, should be in range [0.0, 1.0].

Available additional parameters for AAS animator:
* **`Upright`:** defines linear coefficient between current viewpoint height and avatar's viewpoint height. Range - [0.0,1.0] (0.0 - floor, 1.0 - full standing).
  * Note: can be set as local-only (not synced) if starts with `#` character.
  
## Example of usage in AAS animator for mixed desktop and VR
* To differentiate between desktop and VR players use `CVR Parameter Stream` component on avatar's root gameobject. As example, `InVR` and `InFBT` are boolean typed animator parameters:  
![](.github/img_02.png)
* Add additional transitions between standing, crouching and proning blend trees:  
![](.github/img_03.png)
* Add conditions for new VR transitions:  
  * Standing -> Crouching:  
  ![](.github/img_04.png)
  * Crouching -> Standing:  
  ![](.github/img_05.png)
  * Crouching -> Proning:  
  ![](.github/img_06.png)
  * Proning -> Crouching:  
  ![](.github/img_07.png)
* Add condition check for all desktop transitions:  
![](.github/img_08.png)
  
# Notes
* Sometimes after restoring legs autostep avatar's torso shakes, currently investigating solution.
