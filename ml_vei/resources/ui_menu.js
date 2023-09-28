// Add settings
var g_modSettingsVEI = [];

engine.on('updateModSettingVEI', function (_name, _value) {
    for (var i = 0; i < g_modSettingsVEI.length; i++) {
        if (g_modSettingsVEI[i].name == _name) {
            g_modSettingsVEI[i].updateValue(_value);
            break;
        }
    }
});

// Add own menu
{
    let l_block = document.createElement('div');
    l_block.innerHTML = `
        <div class ="settings-subcategory">
            <div class ="subcategory-name">Vive Extended Input</div>
            <div class ="subcategory-description"></div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Disable gestures while moving: </div>
            <div class ="option-input">
                <div id="Gestures" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>
    `;
    document.getElementById('settings-input').appendChild(l_block);

    // Update toggles in new menu block
    let l_toggles = l_block.querySelectorAll('.inp_toggle');
    for (var i = 0; i < l_toggles.length; i++) {
        g_modSettingsVEI[g_modSettingsVEI.length] = new inp_toggle_mod(l_toggles[i], 'MelonMod_VEI_Call_InpToggle');
    }
}
