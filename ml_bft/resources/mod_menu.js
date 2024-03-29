{
    let l_block = document.createElement('div');
    l_block.innerHTML = `
        <div class ="settings-subcategory">
            <div class ="subcategory-name">Better Fingers Tracking</div>
            <div class ="subcategory-description"></div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Force SteamVR skeletal input: </div>
            <div class ="option-input">
                <div id="SkeletalInput" class ="inp_toggle no-scroll" data-current="false"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Motion range: </div>
            <div class ="option-input">
                <div id="MotionRange" class ="inp_dropdown no-scroll" data-options="0:With controller,1:Without controller" data-current="0"></div>
            </div>
        </div>
        
        <div class ="row-wrapper">
            <div class ="option-caption">Show hands model: </div>
            <div class ="option-input">
                <div id="ShowHands" class ="inp_toggle no-scroll" data-current="false"></div>
            </div>
        </div>
    `;
    document.getElementById('settings-input').appendChild(l_block);

    // Toggles
    for (let l_toggle of l_block.querySelectorAll('.inp_toggle'))
        modsExtension.addSetting('BFT', l_toggle.id, modsExtension.createToggle(l_toggle, 'OnToggleUpdate_BFT'));

    // Dropdowns
    for (let l_dropdown of l_block.querySelectorAll('.inp_dropdown'))
        modsExtension.addSetting('BFT', l_dropdown.id, modsExtension.createDropdown(l_dropdown, 'OnDropdownUpdate_BFT'));
}
