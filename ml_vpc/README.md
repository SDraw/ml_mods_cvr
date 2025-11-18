# Video Player Cookies
This mod allows yt-dlp to use cookies for playing YouTube videos.

# Installation
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `VideoPlayerCookies.dll` in `Mods` folder of game

# Usage
Available mod's settings in `Settings - General - Video Player Cookies`:
* **Enabled:** Whether this mod adds cookie parameters or not; `true` by default.
* **Cookie fetch mode:** cookies fetch method; `Cookie text file` by default.
  * **Cookie text file** *(default)* fetches cookies from your `cookies.txt` file, check [How to create cookies.txt](#how-to-create-cookiestxt)
  * **Browser Firefox** fetches cookies directly from FireFox browser. *requires to be logged-in on YouTube in FireFox*
  * **Browser Brave** fetches cookies directly from Brave browser. *requires to be logged-in on YouTube in Brave*
  * **Browser Chrome** fetches cookies directly from Chrome browser. *requires to be logged-in on YouTube in Chrome*
  * **Browser Chromium** fetches cookies directly from Chromium browser. *requires to be logged-in on YouTube in Chromium*
  * **Browser Edge** fetches cookies directly from Edge browser. *requires to be logged-in on YouTube in Edge*
  * **Browser Opera** fetches cookies directly from Opera browser. *requires to be logged-in on YouTube in Opera*
  * **Browser Safari** fetches cookies directly from Safari browser. *requires to be logged-in on YouTube in Safari*
  * **Browser Vivaldi** fetches cookies directly from Vivaldi browser. *requires to be logged-in on YouTube in Vivaldi*
  * **Browser Whale** fetches cookies directly from Whale browser. *requires to be logged-in on YouTube in Whale*

# How to create cookies.txt
* Acquire cookies for YouTube from your browser:
  * Chromium-based browsers: [Get cookies.txt LOCALLY](https://chromewebstore.google.com/detail/get-cookiestxt-locally/cclelndahbckbenkjhflpdbgdldlbecc) extension
  * Firefox-based browsers: [cookies.txt](https://addons.mozilla.org/en-US/firefox/addon/cookies-txt) extension
* Save result as file named `cookies.txt` in `<game_folder>/UserData` folder

# Notes
* After first use yt-dlp will remove unnecessary cookies from file automatically.
* Cookies contain private information and access to your YouTube account, **do not share them to anyone**.
