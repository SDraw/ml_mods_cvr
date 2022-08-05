# Additional Avatar Parameters
This mod adds additional paramaters for usage in avatar's animator.

# Installation
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `ml_aap.dll` in `Mods` folder of game
  
# Usage
List of new parameters that can be added to your AAS animator:
| Name | Type | Note |
|------|------|------|
| Upright | float | Proportion value between avatar's viewpoint height and floor, ranged in [0,1] |
| Viseme | int | Most active viseme index, ranged in [0,14], doesn't update in offline rooms |
| Voice | float | Voice level, ranged in [0,1], doesn't update in offline rooms |
| Muted | bool | Indicates if microphone is muted or unmuted |
| InVR | bool | Indicates if player is in VR |
| InHmd | bool | Indicates if players' headset is on head, can vary between different VR headsets |
| InFBT | bool | Indicates if player is in full body tracking mode |
| Zoom | float | Zoom level of camera, ranged in [0,1], desktop only |

# Notes
* All new parameters use additional sync data besides listed in avatar's advanced settings.
  * If character `#` is added at start of parameter's name it will be interpreted as local-only, won't be synced over network and won't use additional sync data.
