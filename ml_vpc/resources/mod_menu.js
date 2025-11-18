{
    let l_block = document.createElement('div');
    l_block.innerHTML = `
        <div class ="settings-subcategory">
            <div class ="subcategory-name">Video Player Cookies</div>
            <div class ="subcategory-description"></div>
        </div>
        
        <div class ="row-wrapper">
            <div class ="option-caption" data-tooltip="Whether the mod is enabled or not">Enabled: </div>
            <div class ="option-input">
                <div id="Enabled" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption" data-tooltip="The way cookies are fetched">Cookie fetch mode: </div>
            <div class ="option-input">
                <div id="Mode" class ="inp_dropdown no-scroll" data-options="0:Cookie text file,1:Browser Firefox,2:Browser Brave,3:Browser Chrome,4:Browser Chromium,5:Browser Edge,6:Browser Opera,7:Browser Safari,8:Browser Vivaldi,9:Browser Whale" data-current="0"></div>
            </div>
        </div>
    `;

    document.getElementById('settings-general').appendChild(l_block);

    // Toggles
    for (let l_toggle of l_block.querySelectorAll('.inp_toggle'))
        modsExtension.addSetting('VPC', l_toggle.id, modsExtension.createToggle(l_toggle, 'OnToggleUpdate_VPC'));

    // Sliders
    for (let l_slider of l_block.querySelectorAll('.inp_slider'))
        modsExtension.addSetting('VPC', l_slider.id, modsExtension.createSlider(l_slider, 'OnSliderUpdate_VPC'));

    // Dropdowns
    for (let l_dropdown of l_block.querySelectorAll('.inp_dropdown'))
        modsExtension.addSetting('VPC', l_dropdown.id, modsExtension.createDropdown(l_dropdown, 'OnDropdownUpdate_VPC'));
}
