// Add own menu
{
    let l_block = document.createElement('div');
    l_block.innerHTML = `
        <div class ="settings-subcategory">
            <div class ="subcategory-name">Desktop Head Tracking</div>
            <div class ="subcategory-description"></div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Enabled: </div>
            <div class ="option-input">
                <div id="Enabled" class ="inp_toggle no-scroll" data-current="false"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Use head tracking: </div>
            <div class ="option-input">
                <div id="HeadTracking" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Use eyes tracking: </div>
            <div class ="option-input">
                <div id="EyeTracking" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Use face tracking: </div>
            <div class ="option-input">
                <div id="FaceTracking" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Use blinking: </div>
            <div class ="option-input">
                <div id="Blinking" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>
        
        <div class ="row-wrapper">
            <div class ="option-caption">Mirrored movement: </div>
            <div class ="option-input">
                <div id="Mirrored" class ="inp_toggle no-scroll" data-current="false"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Movement smoothing: </div>
            <div class ="option-input">
                <div id="Smoothing" class ="inp_slider no-scroll" data-min="0" data-max="99" data-current="50"></div>
            </div>
        </div>

        
    `;
    document.getElementById('settings-implementation').appendChild(l_block);

    // Toggles
    for (let l_toggle of l_block.querySelectorAll('.inp_toggle'))
        modsExtension.addSetting('DHT', l_toggle.id, modsExtension.createToggle(l_toggle, 'OnToggleUpdate_DHT'));

    // Sliders
    for (let l_slider of l_block.querySelectorAll('.inp_slider'))
        modsExtension.addSetting('DHT', l_slider.id, modsExtension.createSlider(l_slider, 'OnSliderUpdate_DHT'));
}
