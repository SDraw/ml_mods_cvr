var l_block = document.createElement("lme_block");
l_block.innerHTML = `
    <h2>Leap Motion tracking</h2>
    <div class ="row-wrapper">
        <div class ="option-caption">Enable tracking: </div>
        <div class ="option-input">
            <div id="InteractionLeapMotionTracking" class ="inp_toggle" data-current="false" data-saveOnChange="true"></div>
        </div>
    </div>

    <div class ="row-wrapper">
        <div class ="option-caption">Tracking mode: </div>
        <div class ="option-input">
            <div id="InteractionLeapMotionTrackingMode" class ="inp_dropdown" data-options="0:Screentop,1:Desktop,2:HMD" data-current="1" data-saveOnChange="true"></div>
        </div>
    </div>

    <div class ="row-wrapper">
        <div class ="option-caption">Desktop offset X: </div>
        <div class ="option-input">
            <div id="InteractionLeapMotionTrackingDesktopX" class ="inp_slider" data-min="-100" data-max="100" data-current="0" data-saveOnChange="true" data-continuousUpdate="true"></div>
        </div>
    </div>

    <div class ="row-wrapper">
        <div class ="option-caption">Desktop offset Y: </div>
        <div class ="option-input">
            <div id="InteractionLeapMotionTrackingDesktopY" class ="inp_slider" data-min="-100" data-max="100" data-current="-45" data-saveOnChange="true" data-continuousUpdate="true"></div>
        </div>
    </div>

    <div class ="row-wrapper">
        <div class ="option-caption">Desktop offset Z: </div>
        <div class ="option-input">
            <div id="InteractionLeapMotionTrackingDesktopZ" class ="inp_slider" data-min="-100" data-max="100" data-current="30" data-saveOnChange="true" data-continuousUpdate="true"></div>
        </div>
    </div>

    <div class ="row-wrapper">
        <div class ="option-caption">Attach to head: </div>
        <div class ="option-input">
            <div id="InteractionLeapMotionTrackingHead" class ="inp_toggle" data-current="false" data-saveOnChange="true"></div>
        </div>
    </div>

    <div class ="row-wrapper">
        <div class ="option-caption">Head offset X: </div>
        <div class ="option-input">
            <div id="InteractionLeapMotionTrackingHeadX" class ="inp_slider" data-min="-100" data-max="100" data-current="0" data-saveOnChange="true" data-continuousUpdate="true"></div>
        </div>
    </div>

    <div class ="row-wrapper">
        <div class ="option-caption">Head offset Y: </div>
        <div class ="option-input">
            <div id="InteractionLeapMotionTrackingHeadY" class ="inp_slider" data-min="-100" data-max="100" data-current="-30" data-saveOnChange="true" data-continuousUpdate="true"></div>
        </div>
    </div>

    <div class ="row-wrapper">
        <div class ="option-caption">Head offset Z: </div>
        <div class ="option-input">
            <div id="InteractionLeapMotionTrackingHeadZ" class ="inp_slider" data-min="-100" data-max="100" data-current="15" data-saveOnChange="true" data-continuousUpdate="true"></div>
        </div>
    </div>

    <div class ="row-wrapper">
        <div class ="option-caption">Offset angle: </div>
        <div class ="option-input">
            <div id="InteractionLeapMotionTrackingAngle" class ="inp_slider" data-min="-180" data-max="180" data-current="0" data-saveOnChange="true" data-continuousUpdate="true"></div>
        </div>
    </div>

    <div class ="row-wrapper">
        <div class ="option-caption">Fingers tracking only: </div>
        <div class ="option-input">
            <div id="InteractionLeapMotionTrackingFingersOnly" class ="inp_toggle" data-current="false" data-saveOnChange="true"></div>
        </div>
    </div>

    <div class ="row-wrapper">
        <div class ="option-caption">Model visibility: </div>
        <div class ="option-input">
            <div id="InteractionLeapMotionTrackingModel" class ="inp_toggle" data-current="false" data-saveOnChange="true"></div>
        </div>
    </div>
`;
document.getElementById('settings-implementation').appendChild(l_block);

// Update toggles in new menu block
var l_toggles = l_block.querySelectorAll('.inp_toggle');
for (var i = 0; i < l_toggles.length; i++) {
    settings[settings.length] = new inp_toggle(l_toggles[i]);
}

//Update dropdowns in new menu block
var l_dropdowns = l_block.querySelectorAll('.inp_dropdown');
for (var i = 0; i < l_dropdowns.length; i++) {
    settings[settings.length] = new inp_dropdown(l_dropdowns[i]);
}

// Update sliders in new menu block
var l_sliders = l_block.querySelectorAll('.inp_slider');
for (var i = 0; i < l_sliders.length; i++) {
    settings[settings.length] = new inp_slider(l_sliders[i]);
}
