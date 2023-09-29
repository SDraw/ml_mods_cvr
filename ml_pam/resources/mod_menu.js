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

    // Toggles
    for (let l_toggle of l_block.querySelectorAll('.inp_toggle'))
        modsExtension.addSetting('PAM', l_toggle.id, modsExtension.createToggle(l_toggle, 'OnToggleUpdate_PAM'));

    // Sliders
    for (let l_slider of l_block.querySelectorAll('.inp_slider'))
        modsExtension.addSetting('PAM', l_slider.id, modsExtension.createSlider(l_slider, 'OnSliderUpdate_PAM'));
}
