# Player Ragdoll Mod
This mod turns player's avatar into ragdoll puppet.

# Installation
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `ml_prm.dll` in `Mods` folder of game
  
# Usage
* Press `R` to turn into ragdoll and back.

Optional mod's settings with [BTKUILib](https://github.com/BTK-Development/BTKUILib):
* **Switch ragdoll:** turns into ragdoll state and back, made for VR usage primarily.
* **Use hotkey:** enables/disables ragdoll state switch with `R` key; `true` by default.
* **Restore position:** returns to position of ragdoll state activation upon ragdoll state exit; `false` by default.
* **Use gravity:** enables/disables gravity for ragdoll; `true` by default.
* **Velocity multiplier:** velocity force multiplier based on player's movement direction; `2.0` by default.
* **Movement drag:** movement resistance; `1.0` by default.
* **Angular movement drag:** angular movement resistance; `0.5` by default.
* **Reset settings:** resets mod settings to default.

# Notes
* Incompatible with `Follow hips on IK override` option in AvatarMotionTweaker.
* Not suggested to activate fly mode with enabled ragdoll state.
* Can't be activated in worlds that don't allow flying and spawnables.
* If ragdoll state is enabled in during emote, remote players see whole emote playing while local player sees ragdolling. It's tied to how game handles remote players, currently can be prevented with renaming avatar emote animations to not have default name or containing `Emote` substring.

# Unity Editor Script
You can also trigger the ragdoll via animations on your avatar. To do this you need to download and import the 
`ml_prm_editor_script.unitypackage` into your unity project. Then add the component `Ragdoll Toggle` anywhere inside of
your avatar's hierarchy. Now you can animate both parameters available:

- **Should Override:** Whether the animation should override the toggled state of the ragdoll.
- **Is On:** Whether the ragdoll state is On or Off (only works if `Should Override` is also On).

![](resources/ragdoll_toggle_editor_script.png)

**Note:** In order to work the game object needs to be active and the component enabled.
