// Modified from original `inp` types, because I have no js knowledge to hook stuff

if (typeof inp_toggle_mod === 'undefined') {
    window.inp_toggle_mod = function (_obj, _callbackName) {
        this.obj = _obj;
        this.callbackName = _callbackName;
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

            engine.call(self.callbackName, self.name, self.value);
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
    };
}

if (typeof inp_slider_mod === 'undefined') {
    window.inp_slider_mod = function (_obj, _callbackName) {
        this.obj = _obj;
        this.callbackName = _callbackName;
        this.minValue = parseFloat(_obj.getAttribute('data-min'));
        this.maxValue = parseFloat(_obj.getAttribute('data-max'));
        this.percent = 0;
        this.value = parseFloat(_obj.getAttribute('data-current'));
        this.dragActive = false;
        this.name = _obj.id;
        this.type = _obj.getAttribute('data-type');
        this.stepSize = _obj.getAttribute('data-stepSize') || 0;
        this.format = _obj.getAttribute('data-format') || '{value}';

        var self = this;

        if (this.stepSize != 0)
            this.value = Math.round(this.value / this.stepSize) * this.stepSize;
        else
            this.value = Math.round(this.value);

        this.valueLabelBackground = document.createElement('div');
        this.valueLabelBackground.className = 'valueLabel background';
        this.valueLabelBackground.innerHTML = this.format.replace('{value}', this.value);
        this.obj.appendChild(this.valueLabelBackground);

        this.valueBar = document.createElement('div');
        this.valueBar.className = 'valueBar';
        this.valueBar.setAttribute('style', 'width: ' + (((this.value - this.minValue) / (this.maxValue - this.minValue)) * 100) + '%;');
        this.obj.appendChild(this.valueBar);

        this.valueLabelForeground = document.createElement('div');
        this.valueLabelForeground.className = 'valueLabel foreground';
        this.valueLabelForeground.innerHTML = this.format.replace('{value}', this.value);
        this.valueLabelForeground.setAttribute('style', 'width: ' + (1.0 / ((this.value - this.minValue) / (this.maxValue - this.minValue)) * 100) + '%;');
        this.valueBar.appendChild(this.valueLabelForeground);

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
    };
}

if (typeof inp_dropdown_mod === 'undefined') {
    window.inp_dropdown_mod = function (_obj, _callbackName) {
        this.obj = _obj;
        this.callbackName = _callbackName;
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

            engine.call(self.callbackName, self.name, self.value);
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
    };
}
