# Leap Motion Extension
This mod allows you to use your Leap Motion controller for hands and fingers visual tracking.

# Installation
* Install [latest Ultraleap Gemini tracking software](https://developer.leapmotion.com/tracking-software-download)
* Install [latest MelonLoader](https://github.com/LavaGang/MelonLoader)
* Get [latest release DLL](../../../releases/latest):
  * Put `ml_lme_cvr.dll` in `Mods` folder of game
* Add code section below in `<GameFolder>\ChilloutVR_Data\StreamingAssets\Cohtml\UIResources\CVRTest\index.html` after div for `InteractionViveFaceTrackingStrength`:
```html
<!--Leap Motion start-->
<h2>Leap Motion tracking</h2>
<div class="row-wrapper">
    <div class="option-caption">Enable tracking:</div>
    <div class="option-input">
        <div id="InteractionLeapMotionTracking" class="inp_toggle" data-current="false" data-saveOnChange="true"></div>
    </div>
</div>

<div class="row-wrapper">
    <div class="option-caption">Desktop offset X:</div>
    <div class="option-input">
        <div id="InteractionLeapMotionTrackingDesktopX" class="inp_slider" data-min="-100" data-max="100" data-current="0" data-saveOnChange="true" data-continuousUpdate="true"></div>
    </div>
</div>

<div class="row-wrapper">
    <div class="option-caption">Desktop offset Y:</div>
    <div class="option-input">
        <div id="InteractionLeapMotionTrackingDesktopY" class="inp_slider" data-min="-100" data-max="100" data-current="-45" data-saveOnChange="true" data-continuousUpdate="true"></div>
    </div>
</div>

<div class="row-wrapper">
    <div class="option-caption">Desktop offset Z:</div>
    <div class="option-input">
        <div id="InteractionLeapMotionTrackingDesktopZ" class="inp_slider" data-min="-100" data-max="100" data-current="30" data-saveOnChange="true" data-continuousUpdate="true"></div>
    </div>
</div>

<div class="row-wrapper">
    <div class="option-caption">Fingers tracking only:</div>
    <div class="option-input">
        <div id="InteractionLeapMotionTrackingFingersOnly" class="inp_toggle" data-current="false" data-saveOnChange="true"></div>
    </div>
</div>
<!--Leap Motion end-->
```

# Usage
## Settings
Available mod's settings in `Settings - Implementation`:
* **Enable tracking:** enable hands tracking from Leap Motion data, disabled by default.
* **Desktop offset X/Y/Z:** offset position for body attachment, (0, -45, 30) by default.
* **Fingers tracking only:** apply only fingers tracking, disabled by default.

# Notes
* Only desktop mode is implemented, VR mode is in development.
* Head attachment isn't implemented, in development.
* Root rotation isn't implemented, in development.
* Model visibility isn't implemented, in development.
