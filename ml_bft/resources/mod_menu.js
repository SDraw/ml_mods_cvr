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
}
