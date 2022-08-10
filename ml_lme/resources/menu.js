// Add settings
var g_modSettings = [];

engine.on('updateModSetting', function (_name, _value) {
    for (var i = 0; i < g_modSettings.length; i++) {
        if (g_modSettings[i].name == _name) {
            g_modSettings[i].updateValue(_value);
            break;
        }
    }
});

// Modified from original `inp` types
function inp_toggle_mod(_obj) {
    this.obj = _obj;
    this.value = _obj.getAttribute('data-current');
    this.name = _obj.id;
    this.type = _obj.getAttribute('data-type');

    var self = this;

    this.mouseDown = function (_e) {
        self.value = self.value == "True" ? "False" : "True";
        self.updateState();
    }

    this.updateState = function () {
        self.obj.classList.remove("checked");
        if (self.value == "True") {
            self.obj.classList.add("checked");
        }

        engine.call('MelonMod_LME_Call_InpToggle', self.name, self.value);
    }

    _obj.addEventListener('mousedown', this.mouseDown);

    this.getValue = function () {
        return self.value;
    }

    this.updateValue = function (value) {
        self.value = value;

        self.obj.classList.remove("checked");
        if (self.value == "True") {
            self.obj.classList.add("checked");
        }
    }

    this.updateValue(this.value);

    return {
        name: this.name,
        value: this.getValue,
        updateValue: this.updateValue
    }
}

function inp_slider_mod(_obj) {
    this.obj = _obj;
    this.minValue = parseFloat(_obj.getAttribute('data-min'));
    this.maxValue = parseFloat(_obj.getAttribute('data-max'));
    this.percent = 0;
    this.value = parseFloat(_obj.getAttribute('data-current'));
    this.dragActive = false;
    this.name = _obj.id;
    this.type = _obj.getAttribute('data-type');
    this.caption = _obj.getAttribute('data-caption');
    this.continuousUpdate = _obj.getAttribute('data-continuousUpdate');

    var self = this;

    this.valueBar = document.createElement('div');
    this.valueBar.className = 'valueBar';
    this.valueBar.setAttribute('style', 'width: ' + (((this.value - this.minValue) / (this.maxValue - this.minValue)) * 100) + '%;');
    this.obj.appendChild(this.valueBar);

    this.valueLabel = document.createElement('div');
    this.valueLabel.className = 'valueLabel';
    this.valueLabel.innerHTML = this.caption + Math.round(this.value);
    this.obj.appendChild(this.valueLabel);

    this.mouseDown = function (_e) {
        self.dragActive = true;
        self.mouseMove(_e, false);
    }

    this.mouseMove = function (_e, _write) {
        if (self.dragActive) {
            var rect = _obj.getBoundingClientRect();
            var start = rect.left;
            var end = rect.right;
            self.percent = Math.min(Math.max((_e.clientX - start) / rect.width, 0), 1);
            var value = self.percent;
            value *= (self.maxValue - self.minValue);
            value += self.minValue;
            self.value = Math.round(value);

            self.valueBar.setAttribute('style', 'width: ' + (self.percent * 100) + '%;');
            self.valueLabel.innerHTML = self.caption + self.value;

            if (_write === true || self.continuousUpdate == 'true') {
                engine.call('MelonMod_LME_Call_InpSlider', self.name, "" + self.value);
                self.displayImperial();
            }
        }
    }

    this.mouseUp = function (_e) {
        self.mouseMove(_e, true);
        self.dragActive = false;
    }

    _obj.addEventListener('mousedown', this.mouseDown);
    document.addEventListener('mousemove', this.mouseMove);
    document.addEventListener('mouseup', this.mouseUp);

    this.getValue = function () {
        return self.value;
    }

    this.updateValue = function (value) {
        self.value = Math.round(value);
        self.percent = (self.value - self.minValue) / (self.maxValue - self.minValue);
        self.valueBar.setAttribute('style', 'width: ' + (self.percent * 100) + '%;');
        self.valueLabel.innerHTML = self.caption + self.value;
        self.displayImperial();
    }

    this.displayImperial = function () {
        var displays = document.querySelectorAll('.imperialDisplay');
        for (var i = 0; i < displays.length; i++) {
            var binding = displays[i].getAttribute('data-binding');
            if (binding == self.name) {
                var realFeet = ((self.value * 0.393700) / 12);
                var feet = Math.floor(realFeet);
                var inches = Math.floor((realFeet - feet) * 12);
                displays[i].innerHTML = feet + "&apos;" + inches + '&apos;&apos;';
            }
        }
    }

    return {
        name: this.name,
        value: this.getValue,
        updateValue: this.updateValue
    }
}

function inp_dropdown_mod(_obj) {
    this.obj = _obj;
    this.value = _obj.getAttribute('data-current');
    this.options = _obj.getAttribute('data-options').split(',');
    this.name = _obj.id;
    this.opened = false;
    this.keyValue = [];
    this.type = _obj.getAttribute('data-type');

    this.optionElements = [];

    var self = this;

    this.SelectValue = function (_e) {
        self.value = _e.target.getAttribute('data-key');
        self.valueElement.innerHTML = _e.target.getAttribute('data-value');
        self.globalClose();

        engine.call('MelonMod_LME_Call_InpDropdown', self.name, self.value);
    }

    this.openClick = function (_e) {
        if (self.obj.classList.contains('open')) {
            self.obj.classList.remove('open');
            self.list.setAttribute('style', 'display: none;');
        } else {
            self.obj.classList.add('open');
            self.list.setAttribute('style', 'display: block;');
            self.opened = true;
            window.setTimeout(function () { self.opened = false; }, 10);
        }
    }

    this.globalClose = function (_e) {
        if (self.opened) return;
        self.obj.classList.remove('open');
        self.list.setAttribute('style', 'display: none;');
    }

    this.list = document.createElement('div');
    this.list.className = 'valueList';

    this.updateOptions = function () {
        self.list.innerHTML = "";
        for (var i = 0; i < self.options.length; i++) {
            self.optionElements[i] = document.createElement('div');
            self.optionElements[i].className = 'listValue';
            var valuePair = Array.isArray(self.options[i]) ? self.options[i] : self.options[i].split(':');
            var key = "";
            var value = "";
            if (valuePair.length == 1) {
                key = valuePair[0];
                value = valuePair[0];
            } else {
                key = valuePair[0];
                value = valuePair[1];
            }
            self.keyValue[key] = value;
            self.optionElements[i].innerHTML = value;
            self.optionElements[i].setAttribute('data-value', value);
            self.optionElements[i].setAttribute('data-key', key);
            self.list.appendChild(self.optionElements[i]);
            self.optionElements[i].addEventListener('mousedown', self.SelectValue);
        }

        self.valueElement.innerHTML = self.keyValue[self.value];
    }

    this.valueElement = document.createElement('div');
    this.valueElement.className = 'dropdown-value';

    this.updateOptions();

    this.obj.appendChild(this.valueElement);
    this.obj.appendChild(this.list);
    this.valueElement.addEventListener('mousedown', this.openClick);
    document.addEventListener('mousedown', this.globalClose);

    this.getValue = function () {
        return self.value;
    }

    this.updateValue = function (value) {
        self.value = value;
        self.valueElement.innerHTML = self.keyValue[value];
    }

    this.setOptions = function (options) {
        self.options = options;
    }

    return {
        name: this.name,
        value: this.getValue,
        updateValue: this.updateValue,
        updateOptions: this.updateOptions,
        setOptions: this.setOptions
    }
}

// Add own menu
{
    var l_block = document.createElement('div');
    l_block.innerHTML = `
        <h2>Leap Motion tracking</h2>
        <div class ="row-wrapper">
            <div class ="option-caption">Enable tracking: </div>
            <div class ="option-input">
                <div id="Enabled" class ="inp_toggle" data-current="false" data-saveOnChange="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Tracking mode: </div>
            <div class ="option-input">
                <div id="Mode" class ="inp_dropdown" data-options="0:Screentop,1:Desktop,2:HMD" data-current="1" data-saveOnChange="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Desktop offset X: </div>
            <div class ="option-input">
                <div id="DesktopX" class ="inp_slider" data-min="-100" data-max="100" data-current="0" data-saveOnChange="true" data-continuousUpdate="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Desktop offset Y: </div>
            <div class ="option-input">
                <div id="DesktopY" class ="inp_slider" data-min="-100" data-max="100" data-current="-45" data-saveOnChange="true" data-continuousUpdate="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Desktop offset Z: </div>
            <div class ="option-input">
                <div id="DesktopZ" class ="inp_slider" data-min="-100" data-max="100" data-current="30" data-saveOnChange="true" data-continuousUpdate="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Attach to head: </div>
            <div class ="option-input">
                <div id="Head" class ="inp_toggle" data-current="false" data-saveOnChange="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Head offset X: </div>
            <div class ="option-input">
                <div id="HeadX" class ="inp_slider" data-min="-100" data-max="100" data-current="0" data-saveOnChange="true" data-continuousUpdate="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Head offset Y: </div>
            <div class ="option-input">
                <div id="HeadY" class ="inp_slider" data-min="-100" data-max="100" data-current="-30" data-saveOnChange="true" data-continuousUpdate="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Head offset Z: </div>
            <div class ="option-input">
                <div id="HeadZ" class ="inp_slider" data-min="-100" data-max="100" data-current="15" data-saveOnChange="true" data-continuousUpdate="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Offset angle: </div>
            <div class ="option-input">
                <div id="Angle" class ="inp_slider" data-min="-180" data-max="180" data-current="0" data-saveOnChange="true" data-continuousUpdate="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Fingers tracking only: </div>
            <div class ="option-input">
                <div id="FingersOnly" class ="inp_toggle" data-current="false" data-saveOnChange="true"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Model visibility: </div>
            <div class ="option-input">
                <div id="Model" class ="inp_toggle" data-current="false" data-saveOnChange="true"></div>
            </div>
        </div>
    `;
    document.getElementById('settings-implementation').appendChild(l_block);

    // Update toggles in new menu block
    var l_toggles = l_block.querySelectorAll('.inp_toggle');
    for (var i = 0; i < l_toggles.length; i++) {
        g_modSettings[g_modSettings.length] = new inp_toggle_mod(l_toggles[i]);
    }

    //Update dropdowns in new menu block
    var l_dropdowns = l_block.querySelectorAll('.inp_dropdown');
    for (var i = 0; i < l_dropdowns.length; i++) {
        g_modSettings[g_modSettings.length] = new inp_dropdown_mod(l_dropdowns[i]);
    }

    // Update sliders in new menu block
    var l_sliders = l_block.querySelectorAll('.inp_slider');
    for (var i = 0; i < l_sliders.length; i++) {
        g_modSettings[g_modSettings.length] = new inp_slider_mod(l_sliders[i]);
    }
}

