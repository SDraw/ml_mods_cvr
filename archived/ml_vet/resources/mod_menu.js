{
    let l_block = document.createElement('div');
    l_block.innerHTML = `
        <div class ="settings-subcategory">
            <div class ="subcategory-name">Vive Eye Tracking</div>
            <div class ="subcategory-description"></div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Enable eye tracking: </div>
            <div class ="option-input">
                <div id="Enabled" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>
        <div class="row-wrapper">
            <p>Requires Vive Face tracking restart at first.</p>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Gaze smoothing: </div>
            <div class ="option-input">
                <div id="Smoothing" class ="inp_slider no-scroll" data-min="0" data-max="100" data-current="5"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Debug gizmos: </div>
            <div class ="option-input">
                <div id="Debug" class ="inp_toggle no-scroll" data-current="false"></div>
            </div>
        </div>
    `;
    document.getElementById('settings-implementation').appendChild(l_block);

    // Toggles
    for (let l_toggle of l_block.querySelectorAll('.inp_toggle'))
        modsExtension.addSetting('VET', l_toggle.id, modsExtension.createToggle(l_toggle, 'OnToggleUpdate_VET'));

    // Sliders
    for (let l_slider of l_block.querySelectorAll('.inp_slider'))
        modsExtension.addSetting('VET', l_slider.id, modsExtension.createSlider(l_slider, 'OnSliderUpdate_VET'));
}
