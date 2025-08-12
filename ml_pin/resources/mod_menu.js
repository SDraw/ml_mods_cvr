{
    let l_block = document.createElement('div');
    l_block.innerHTML = `
        <div class ="settings-subcategory">
            <div class ="subcategory-name">Players Instance Notifier</div>
            <div class ="subcategory-description"></div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Notify of: </div>
            <div class ="option-input">
                <div id="NotifyType" class ="inp_dropdown no-scroll" data-options="0:None,1:Friends,2:All" data-current="2"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Mixed volume: </div>
            <div class ="option-input">
                <div id="Volume" class ="inp_slider no-scroll" data-min="0" data-max="100" data-current="100"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Delay between notifications: </div>
            <div class ="option-input">
                <div id="Delay" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Notify in public instances: </div>
            <div class ="option-input">
                <div id="NotifyInPublic" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Notify in friends instances: </div>
            <div class ="option-input">
                <div id="NotifyInFriends" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Notify in private instances: </div>
            <div class ="option-input">
                <div id="NotifyInPrivate" class ="inp_toggle no-scroll" data-current="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Always notify of friends: </div>
            <div class ="option-input">
                <div id="FriendsAlways" class ="inp_toggle no-scroll" data-current="false"></div>
            </div>
        </div>
    `;
    document.getElementById('settings-audio').appendChild(l_block);

    // Toggles
    for (let l_toggle of l_block.querySelectorAll('.inp_toggle'))
        modsExtension.addSetting('PIN', l_toggle.id, modsExtension.createToggle(l_toggle, 'OnToggleUpdate_PIN'));

    // Sliders
    for (let l_slider of l_block.querySelectorAll('.inp_slider'))
        modsExtension.addSetting('PIN', l_slider.id, modsExtension.createSlider(l_slider, 'OnSliderUpdate_PIN'));

    // Dropdowns
    for (let l_dropdown of l_block.querySelectorAll('.inp_dropdown'))
        modsExtension.addSetting('PIN', l_dropdown.id, modsExtension.createDropdown(l_dropdown, 'OnDropdownUpdate_PIN'));
}
