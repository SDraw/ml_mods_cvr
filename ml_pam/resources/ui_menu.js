// Add settings
var g_modSettingsPAM = [];

engine.on('updateModSettingPAM', function (_name, _value) {
    for (var i = 0; i < g_modSettingsPAM.length; i++) {
        if (g_modSettingsPAM[i].name == _name) {
            g_modSettingsPAM[i].updateValue(_value);
            break;
        }
    }
});

// Add own menu
{
    let l_block = document.createElement('div');
    l_block.innerHTML = `
        <div class ="settings-subcategory">
            <div class ="subcategory-name">Pickup Arm Movement</div>
            <div class ="subcategory-description"></div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Enable hand movement: </div>
            <div class ="option-input">
                <div id="Enabled" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Grab offset: </div>
            <div class ="option-input">
                <div id="GrabOffset" class ="inp_slider no-scroll" data-min="0" data-max="100" data-current="25"></div>
            </div>
        </div>
    `;
    document.getElementById('settings-interaction').appendChild(l_block);

    // Update toggles in new menu block
    let l_toggles = l_block.querySelectorAll('.inp_toggle');
    for (var i = 0; i < l_toggles.length; i++) {
        g_modSettingsPAM[g_modSettingsPAM.length] = new inp_toggle_mod(l_toggles[i], 'MelonMod_PAM_Call_InpToggle');
    }

    // Update sliders in new menu block
    let l_sliders = l_block.querySelectorAll('.inp_slider');
    for (var i = 0; i < l_sliders.length; i++) {
        g_modSettingsPAM[g_modSettingsPAM.length] = new inp_slider_mod(l_sliders[i], 'MelonMod_PAM_Call_InpSlider');
    }
}
