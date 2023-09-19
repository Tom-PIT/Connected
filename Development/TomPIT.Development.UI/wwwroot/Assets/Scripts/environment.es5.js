'use strict';

var _createClass = (function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ('value' in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; })();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError('Cannot call a class as a function'); } }

var environment = (function () {
    function environment() {
        _classCallCheck(this, environment);
    }

    _createClass(environment, [{
        key: 'initialize',
        value: function initialize() {
            var _this = this;

            this.loadState();

            var container = document.querySelector('#devExplorerToolbar .btn-group');
            /*
             * copy button
             */
            var copy = document.createElement('button');

            copy.className = 'btn btn-sm btn-light';
            copy.setAttribute('title', 'Copy component');
            copy.addEventListener('click', function (e) {
                _this.copyComponent();
            });

            var copyGlyph = document.createElement('i');

            copyGlyph.className = 'fal fa-copy';

            copy.appendChild(copyGlyph);

            container.appendChild(copy);

            this.copyButton = copy;
            /*
             * paste button
             */
            var paste = document.createElement('button');

            paste.className = 'btn btn-sm btn-light';
            paste.setAttribute('title', 'Paste component');
            paste.addEventListener('click', function (e) {
                _this.pasteComponent();
            });

            var pasteGlyph = document.createElement('i');

            pasteGlyph.className = 'fal fa-paste';

            paste.appendChild(pasteGlyph);

            container.appendChild(paste);

            this.pasteButton = paste;

            ide.element[0].addEventListener('selectionChanged', function (e) {
                _this.syncButtons();
            });

            this.syncButtons();
        }
    }, {
        key: 'copyComponent',
        value: function copyComponent() {
            if (!this.isComponentSelected) return;

            var component = document.querySelector('.dev-explorer-node-content.active').parentElement.getAttribute('data-id');
            var state = {
                component: component
            };

            this.updateState(state);
        }
    }, {
        key: 'pasteComponent',
        value: function pasteComponent() {
            var selection = document.querySelector('.dev-explorer-node-content.active');
            var container = selection.parentElement.getAttribute('data-container');
            var folder = null;

            if (container === 'true' && selection.parentElement.getAttribute('data-static') !== 'true') folder = selection.parentElement.getAttribute('data-id');

            ide.ideAction({
                data: {
                    'action': 'clone',
                    'component': this.state.component,
                    'folder': folder
                },
                onComplete: function onComplete(e) {
                    var path = ide._resolvePath(ide.selectedElement()); //`${ide._resolvePath(ide.selectedElement())}/${e.component}`;

                    ide.refreshSections({
                        sections: 'explorer',
                        path: path
                    });

                    tompit.success('Component pasted from the clipboard.', 'Paste component');
                }
            });
        }
    }, {
        key: 'syncButtons',
        value: function syncButtons() {
            this.copyButton.disabled = !this.isComponentSelected;
            this.pasteButton.disabled = !this.clipboardComponent || this.isComponentSelected;
        }
    }, {
        key: 'loadState',
        value: function loadState() {
            var _this2 = this;

            var url = tompit.DEVDEFAULTS.rootUrl + '/sys/select-user-state';

            this._state = tompit.post({
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: {
                    primaryKey: 'env',
                    topic: 'state'
                },
                url: url,
                onSuccess: function onSuccess(e) {
                    _this2.state = e;
                    _this2.syncButtons();
                }
            });
        }
    }, {
        key: 'updateState',
        value: function updateState(e) {
            this.state = e;

            var url = tompit.DEVDEFAULTS.rootUrl + '/sys/update-user-state';

            tompit.post({
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: {
                    primaryKey: 'env',
                    topic: 'state',
                    value: e
                },
                url: url,
                onSuccess: function onSuccess(e) {
                    tompit.success('Component copied to the clipboard.', 'Copy component');
                }
            });
        }
    }, {
        key: 'clipboardComponent',
        get: function get() {
            if (!this.state) return null;

            return this.state.component;
        }
    }, {
        key: 'isFolderSelected',
        get: function get() {
            var selection = document.querySelector('.dev-explorer-node-content.active');

            if (!selection) return false;

            var container = selection.parentElement.getAttribute('data-container');

            return container === 'true';
        }
    }, {
        key: 'isComponentSelected',
        get: function get() {
            if (this.isFolderSelected) return false;

            var selection = document.querySelector('.dev-explorer-node-content.active');

            if (!selection) return false;

            var isStatic = selection.parentElement.getAttribute('data-static');

            return isStatic === 'false';
        }
    }, {
        key: 'copyButton',
        get: function get() {
            return this._copyButton;
        },
        set: function set(value) {
            this._copyButton = value;
        }
    }, {
        key: 'pasteButton',
        get: function get() {
            return this._pasteButton;
        },
        set: function set(value) {
            this._pasteButton = value;
        }
    }, {
        key: 'state',
        get: function get() {
            return this._state;
        },
        set: function set(value) {
            this._state = value;
        }
    }]);

    return environment;
})();

;

window.env = window.env || new environment();

