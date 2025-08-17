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

        <div class ="row-wrapper">
            <div class ="option-caption">Apply grip/trigger while moving: </div>
            <div class ="option-input">
                <div id="GripTrigger" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Axis priority: </div>
            <div class ="option-input">
                <div id="AxisPriority" class ="inp_dropdown no-scroll" data-options="0:Grip,1:Trigger" data-current="0"></div>
            </div>
        </div>
    `;
    document.getElementById('settings-input').appendChild(l_block);

    // Toggles
    for (let l_toggle of l_block.querySelectorAll('.inp_toggle'))
        modsExtension.addSetting('VEI', l_toggle.id, modsExtension.createToggle(l_toggle, 'OnToggleUpdate_VEI'));

    // Dropdowns
    for (let l_dropdown of l_block.querySelectorAll('.inp_dropdown'))
        modsExtension.addSetting('VEI', l_dropdown.id, modsExtension.createDropdown(l_dropdown, 'OnDropdownUpdate_VEI'));
}
