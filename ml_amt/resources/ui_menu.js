// Add settings
var g_modSettingsAMT = [];

engine.on('updateModSettingAMT', function (_name, _value) {
    for (var i = 0; i < g_modSettingsAMT.length; i++) {
        if (g_modSettingsAMT[i].name == _name) {
            g_modSettingsAMT[i].updateValue(_value);
            break;
        }
    }
});

// Add own menu
{
    let l_block = document.createElement('div');
    l_block.innerHTML = `
        <div class ="settings-subcategory">
            <div class ="subcategory-name">Avatar Motion Tweaker</div>
            <div class ="subcategory-description"></div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Crouch limit: </div>
            <div class ="option-input">
                <div id="CrouchLimit" class ="inp_slider no-scroll" data-min="0" data-max="100" data-current="75"></div>
            </div>
        </div>
        
        <div class ="row-wrapper">
            <div class ="option-caption">Prone limit: </div>
            <div class ="option-input">
                <div id="ProneLimit" class ="inp_slider no-scroll" data-min="0" data-max="100" data-current="40"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">IK override while flying: </div>
            <div class ="option-input">
                <div id="IKOverrideFly" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>
        
        <div class ="row-wrapper">
            <div class ="option-caption">IK override while jumping: </div>
            <div class ="option-input">
                <div id="IKOverrideJump" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Follow hips on IK override: </div>
            <div class ="option-input">
                <div id="FollowHips" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>
        
        <div class ="row-wrapper">
            <div class ="option-caption">Detect animations emote tag: </div>
            <div class ="option-input">
                <div id="DetectEmotes" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Adjusted locomotion mass center: </div>
            <div class ="option-input">
                <div id="MassCenter" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>
    `;
    document.getElementById('settings-ik').appendChild(l_block);

    // Update sliders in new menu block
    let l_sliders = l_block.querySelectorAll('.inp_slider');
    for (var i = 0; i < l_sliders.length; i++) {
        g_modSettingsAMT[g_modSettingsAMT.length] = new inp_slider_mod(l_sliders[i], 'MelonMod_AMT_Call_InpSlider');
    }

    // Update toggles in new menu block
    let l_toggles = l_block.querySelectorAll('.inp_toggle');
    for (var i = 0; i < l_toggles.length; i++) {
        g_modSettingsAMT[g_modSettingsAMT.length] = new inp_toggle_mod(l_toggles[i], 'MelonMod_AMT_Call_InpToggle');
    }
}
