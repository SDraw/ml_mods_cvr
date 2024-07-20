# Pickup Arm Movement
This mod adds arm tracking upon holding pickup in desktop mode.

# Installation
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `ml_pam.dll` in `Mods` folder of game
  
# Usage
Available mod's settings in `Settings - Interactions - Pickup Arm Movement`:
* **Enable hand movement:** enables/disables arm tracking; default value - `true`.
* **Grab offset:** offset from pickup grab point; default value - `25`.
* **Leading hand:** hand that will be extended when gragging pickup; available values: `Left`, `Right`, `Both`; default value - `Right`.
* **Hands extension (Q\E):** extend left and right hand if `Q` and `E` keys are pressed; default value - `true`.

Available animator boolean parameters:
* **LeftHandExtended:`` indicates if left hand is extended.
* **RightHandExtended:`` indicates if right hand is extended.

# Notes
* Made for desktop mode in mind.
