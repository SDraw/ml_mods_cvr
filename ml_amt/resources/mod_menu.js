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

    // Toggles
    for (let l_toggle of l_block.querySelectorAll('.inp_toggle'))
        modsExtension.addSetting('AMT', l_toggle.id, modsExtension.createToggle(l_toggle, 'OnToggleUpdate_AMT'));

    // Sliders
    for (let l_slider of l_block.querySelectorAll('.inp_slider'))
        modsExtension.addSetting('AMT', l_slider.id, modsExtension.createSlider(l_slider, 'OnSliderUpdate_AMT'));
}
