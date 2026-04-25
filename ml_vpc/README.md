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

# How to make it work on Linux (Tested on CachyOS / Arch with Firefox)
Because of Linux / Proton, the cookies.txt doesn't seem to work well, instead it's better to use browser mode, the following tutorial explains how to set it up (tested using Firefox)

* Go to the mod settings and select your browser
* Put a video and wait for it to fail, then look at the logs
  * It should tell you an error because it can't find your cookies in a path, for Firefox it looks like `AppData/Local/Packages/Mozilla.Firefox_n80bbvh6b1yt2/LocalCache/Roaming/Mozilla/Firefox/Profiles/`
* Go to AppData in the drive_c of the game, on Arch this is usually located at `/home/[USER]/.local/share/Steam/steamapps/compatdata/661130/pfx/drive_c/users/steamuser/Appdata`
* Create all the folders that it requires until the last one, for Firefox this means the folders `Packages`, `Mozilla.Firefox_n80bbvh6b1yt2`, `LocalCache`, `Roaming`, `Mozilla` and `Firefox` but don't create the last one (`Profiles`) yet unless you don't intend to use your real browser
* Because your cookies could change or expire, instead of giving it the files once, we'll use a symlink to the real firefox on your machine. This is completely up to you and you could just copy the files or get them from somewhere else
* Now you can create a link that points to the folder of your browser on Linux. For Firefox, this is the `Profiles` folder that should point to `/home/[USER]/.mozilla/firefox/`
* If you followed all the steps, the next video you put should successfully get the cookies from your browser


# Notes
* After first use yt-dlp will remove unnecessary cookies from file automatically.
* Cookies contain private information and access to your YouTube account, **do not share them to anyone**.
