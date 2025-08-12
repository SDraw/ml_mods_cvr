{
    let l_block = document.createElement('div');
    l_block.innerHTML = `
        <div class ="settings-subcategory">
            <div class ="subcategory-name">Pickup Arm Movement</div>
            <div class ="subcategory-description"></div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Enabled: </div>
            <div class ="option-input">
                <div id="Enabled" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Grab offset: </div>
            <div class ="option-input">
                <div id="GrabOffset" class ="inp_slider no-scroll" data-min="0" data-max="100" data-current="50"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Leading hand: </div>
            <div class ="option-input">
                <div id="LeadHand" class ="inp_dropdown no-scroll" data-options="0:Left,1:Right,2:Both" data-current="1"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Hands extension (Q/E): </div>
            <div class ="option-input">
                <div id="HandsExtension" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Hand extension speed: </div>
            <div class ="option-input">
                <div id="ExtensionSpeed" class ="inp_slider no-scroll" data-min="1" data-max="50" data-current="10"></div>
            </div>
        </div>
    `;
    document.getElementById('settings-input').appendChild(l_block);

    // Toggles
    for (let l_toggle of l_block.querySelectorAll('.inp_toggle'))
        modsExtension.addSetting('PAM', l_toggle.id, modsExtension.createToggle(l_toggle, 'OnToggleUpdate_PAM'));

    // Sliders
    for (let l_slider of l_block.querySelectorAll('.inp_slider'))
        modsExtension.addSetting('PAM', l_slider.id, modsExtension.createSlider(l_slider, 'OnSliderUpdate_PAM'));

    // Dropdowns
    for (let l_dropdown of l_block.querySelectorAll('.inp_dropdown'))
        modsExtension.addSetting('PAM', l_dropdown.id, modsExtension.createDropdown(l_dropdown, 'OnDropdownUpdate_PAM'));
}
