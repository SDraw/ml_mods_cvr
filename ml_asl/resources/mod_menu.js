// Add own menu
{
    let l_block = document.createElement('div');
    l_block.innerHTML = `
        <div class ="settings-subcategory">
            <div class ="subcategory-name">Avatar Synced Look</div>
            <div class ="subcategory-description"></div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Enabled: </div>
            <div class ="option-input">
                <div id="Enabled" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>
    `;
    document.getElementById('settings-interaction').appendChild(l_block);

    // Toggles
    for (let l_toggle of l_block.querySelectorAll('.inp_toggle'))
        modsExtension.addSetting('ASL', l_toggle.id, modsExtension.createToggle(l_toggle, 'OnToggleUpdate_ASL'));
}
