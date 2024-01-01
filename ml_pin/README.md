# Players Instance Notifier
This mod implements sound notifications for players joining and leaving.  
This can be considered as attempt of [JoinNotifier](https://github.com/knah/VRCMods/tree/master/JoinNotifier) revival.

# Installation
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `ml_pin.dll` in `Mods` folder of game
  
# Usage
Available mod's settings in `Settings - Audio - Players Instance Notifier`:
* **Notify of:** players notification filter type, available filters: `None`, `Friends`, `All`; `All` by default.
* **Mixed volume:** volume of notifications; `100` by default.
  * Note: Respects game's interface volume setting and mixes with it accordingly.
* **Notify in public instances:** notifies in `Public` instances; `true` by default.
* **Notify in friends instances:** notifies in `Friends of friends` and `Friends` instances; `true` by default.
* **Notify in private instances:** notifies in `Everyone can invite` and `Owner must invite` instances; `true` by default.
* **Always notify of friends:** notifies friends join/leave no matter what; `false` by default.

# Custom notification sounds
You can setup your own notification sounds.  
Go to `<game_folder>/UserData/PlayersInstanceNotifier` and replace to your preferable sounds.

Available sounds for replacement:
* **player_join.wav**
* **player_leave.wav**
* **friend_join.wav**
* **friend_leave.wav**
