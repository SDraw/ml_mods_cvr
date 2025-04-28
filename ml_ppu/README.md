# Player Pick Up
This mod allow you to be picked up and carried around.

# Installation
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Install [BTKUILib](https://github.com/BTK-Development/BTKUILib)
* Get [latest release DLL](../../../releases/latest):
  * Put `PlayerPickUp.dll` in `Mods` folder of game
  
# Usage
Available mod's settings in BTKUILib's page:
* **Enabled:** sets mod's activity as enabled or disabled; `true` by default;
* **Friends only:** allow only friends to pick you up; `true` by default;
* **Velocity multiplier:** velocity multiplier upon drop/throw; `1.0` by default.

To pick you up remote player should:
* Make hands `grab` pointers to appear on your side (usually, press controller grip trigger button or fist gesture, depends on remote player controllers type);
* Touch your avatar's torso with both pointers;

# Notes
* Compatible with PlayerRagdolMod.
