if (typeof modsExtension === 'undefined') {
    window.modsExtension = []

    // UI elements, modified from original `inp` types, because I have no js knowledge to hook stuff
    modsExtension.createToggle = function (_obj, _callbackName) {
        let uiElement = {};

        uiElement.obj = _obj;
        uiElement.callbackName = _callbackName;
        uiElement.value = _obj.getAttribute('data-current');
        uiElement.name = _obj.id;
        uiElement.type = _obj.getAttribute('data-type');

        var self = uiElement;

        uiElement.mouseDown = function (_e) {
            self.value = self.value == "True" ? "False" : "True";
            self.updateState();
        }

        uiElement.updateState = function () {
            self.obj.classList.remove("checked");
            if (self.value == "True") {
                self.obj.classList.add("checked");
            }

            engine.call(self.callbackName, self.name, self.value);
        }

        _obj.addEventListener('mousedown', uiElement.mouseDown);

        uiElement.getValue = function () {
            return self.value;
        }

        uiElement.updateValue = function (value) {
            self.value = value;

            self.obj.classList.remove("checked");
            if (self.value == "True") {
                self.obj.classList.add("checked");
            }
        }

        uiElement.updateValue(uiElement.value);

        return {
            name: uiElement.name,
            value: uiElement.getValue,
            updateValue: uiElement.updateValue
        }
    };

    modsExtension.createSlider = function (_obj, _callbackName) {
        let uiElement = {};

        uiElement.obj = _obj;
        uiElement.callbackName = _callbackName;
        uiElement.minValue = parseFloat(_obj.getAttribute('data-min'));
        uiElement.maxValue = parseFloat(_obj.getAttribute('data-max'));
        uiElement.percent = 0;
        uiElement.value = parseFloat(_obj.getAttribute('data-current'));
        uiElement.dragActive = false;
        uiElement.name = _obj.id;
        uiElement.type = _obj.getAttribute('data-type');
        uiElement.stepSize = _obj.getAttribute('data-stepSize') || 0;
        uiElement.format = _obj.getAttribute('data-format') || '{value}';

        var self = uiElement;

        if (uiElement.stepSize != 0)
            uiElement.value = Math.round(uiElement.value / uiElement.stepSize) * uiElement.stepSize;
        else
            uiElement.value = Math.round(uiElement.value);

        uiElement.valueLabelBackground = document.createElement('div');
        uiElement.valueLabelBackground.className = 'valueLabel background';
        uiElement.valueLabelBackground.innerHTML = uiElement.format.replace('{value}', uiElement.value);
        uiElement.obj.appendChild(uiElement.valueLabelBackground);

        uiElement.valueBar = document.createElement('div');
        uiElement.valueBar.className = 'valueBar';
        uiElement.valueBar.setAttribute('style', 'width: ' + (((uiElement.value - uiElement.minValue) / (uiElement.maxValue - uiElement.minValue)) * 100) + '%;');
        uiElement.obj.appendChild(uiElement.valueBar);

        uiElement.valueLabelForeground = document.createElement('div');
        uiElement.valueLabelForeground.className = 'valueLabel foreground';
        uiElement.valueLabelForeground.innerHTML = uiElement.format.replace('{value}', uiElement.value);
        uiElement.valueLabelForeground.setAttribute('style', 'width: ' + (1.0 / ((uiElement.value - uiElement.minValue) / (uiElement.maxValue - uiElement.minValue)) * 100) + '%;');
        uiElement.valueBar.appendChild(uiElement.valueLabelForeground);

        uiElement.mouseDown = function (_e) {
            self.dragActive = true;
            self.mouseMove(_e, false);
        }

        uiElement.mouseMove = function (_e, _write) {
            if (self.dragActive) {
                var rect = _obj.getBoundingClientRect();
                var start = rect.left;
                var end = rect.right;
                self.percent = Math.min(Math.max((_e.clientX - start) / rect.width, 0), 1);
                var value = self.percent;
                value *= (self.maxValue - self.minValue);
                value += self.minValue;
                if (self.stepSize != 0) {
                    value = Math.round(value / self.stepSize);
                    self.value = value * self.stepSize;
                    self.percent = (self.value - self.minValue) / (self.maxValue - self.minValue);
                }
                else
                    self.value = Math.round(value);

                self.valueBar.setAttribute('style', 'width: ' + (self.percent * 100) + '%;');
                self.valueLabelForeground.setAttribute('style', 'width: ' + (1.0 / self.percent * 100) + '%;');
                self.valueLabelBackground.innerHTML = self.valueLabelForeground.innerHTML = self.format.replace('{value}', self.value);

                engine.call(self.callbackName, self.name, "" + self.value);
                self.displayImperial();
            }
        }

        uiElement.mouseUp = function (_e) {
            self.mouseMove(_e, true);
            self.dragActive = false;
        }

        _obj.addEventListener('mousedown', uiElement.mouseDown);
        document.addEventListener('mousemove', uiElement.mouseMove);
        document.addEventListener('mouseup', uiElement.mouseUp);

        uiElement.getValue = function () {
            return self.value;
        }

        uiElement.updateValue = function (value) {
            if (self.stepSize != 0)
                self.value = Math.round(value * self.stepSize) / self.stepSize;
            else
                self.value = Math.round(value);
            self.percent = (self.value - self.minValue) / (self.maxValue - self.minValue);
            self.valueBar.setAttribute('style', 'width: ' + (self.percent * 100) + '%;');
            self.valueLabelForeground.setAttribute('style', 'width: ' + (1.0 / self.percent * 100) + '%;');
            self.valueLabelBackground.innerHTML = self.valueLabelForeground.innerHTML = self.format.replace('{value}', self.value);
            self.displayImperial();
        }

        uiElement.displayImperial = function () {
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
            name: uiElement.name,
            value: uiElement.getValue,
            updateValue: uiElement.updateValue
        }
    };

    modsExtension.createDropdown = function (_obj, _callbackName) {
        let uiElement = {};

        uiElement.obj = _obj;
        uiElement.callbackName = _callbackName;
        uiElement.value = _obj.getAttribute('data-current');
        uiElement.options = _obj.getAttribute('data-options').split(',');
        uiElement.name = _obj.id;
        uiElement.opened = false;
        uiElement.keyValue = [];
        uiElement.type = _obj.getAttribute('data-type');

        uiElement.optionElements = [];

        var self = uiElement;

        uiElement.SelectValue = function (_e) {
            self.value = _e.target.getAttribute('data-key');
            self.valueElement.innerHTML = _e.target.getAttribute('data-value');
            self.globalClose();

            engine.call(self.callbackName, self.name, self.value);
        }

        uiElement.openClick = function (_e) {
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

        uiElement.globalClose = function (_e) {
            if (self.opened) return;
            self.obj.classList.remove('open');
            self.list.setAttribute('style', 'display: none;');
        }

        uiElement.list = document.createElement('div');
        uiElement.list.className = 'valueList';

        uiElement.updateOptions = function () {
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

        uiElement.valueElement = document.createElement('div');
        uiElement.valueElement.className = 'dropdown-value';

        uiElement.updateOptions();

        uiElement.obj.appendChild(uiElement.valueElement);
        uiElement.obj.appendChild(uiElement.list);
        uiElement.valueElement.addEventListener('mousedown', uiElement.openClick);
        document.addEventListener('mousedown', uiElement.globalClose);

        uiElement.getValue = function () {
            return self.value;
        }

        uiElement.updateValue = function (value) {
            self.value = value;
            self.valueElement.innerHTML = self.keyValue[value];
        }

        uiElement.setOptions = function (options) {
            self.options = options;
        }

        return {
            name: uiElement.name,
            value: uiElement.getValue,
            updateValue: uiElement.updateValue,
            updateOptions: uiElement.updateOptions,
            setOptions: uiElement.setOptions
        }
    };

    modsExtension.settings = []
    modsExtension.settings.data = []; // [category] -> [entry]
    modsExtension.addSetting = function (_category, _entry, _obj) {
        if (modsExtension.settings.data[_category] === undefined)
            modsExtension.settings.data[_category] = []
        modsExtension.settings.data[_category][_entry] = _obj
    };
    modsExtension.updateSetting = function (_category, _entry, _value) {
        if ((modsExtension.settings.data[_category] !== undefined) && (modsExtension.settings.data[_category][_entry] !== undefined))
            modsExtension.settings.data[_category][_entry].updateValue(_value);
    };
    engine.on('updateModSetting', modsExtension.updateSetting);
}
