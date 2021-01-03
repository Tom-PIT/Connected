'use strict';

class environment{
    initialize() {

        this.loadState();

        let container = document.querySelector('#devExplorerToolbar .btn-group');
        /*
         * copy button
         */
        let copy = document.createElement('button');

        copy.className = 'btn btn-sm btn-light';
        copy.setAttribute('title', 'Copy component');
        copy.addEventListener('click', (e) => {
            this.copyComponent();
        });

        let copyGlyph = document.createElement('i');

        copyGlyph.className = 'fal fa-copy';

        copy.appendChild(copyGlyph);

        container.appendChild(copy);

        this.copyButton = copy;
        /*
         * paste button
         */
        let paste = document.createElement('button');

        paste.className = 'btn btn-sm btn-light';
        paste.setAttribute('title', 'Paste component');
        paste.addEventListener('click', (e) => {
            this.pasteComponent();
        });

        let pasteGlyph = document.createElement('i');

        pasteGlyph.className = 'fal fa-paste';

        paste.appendChild(pasteGlyph);

        container.appendChild(paste);

        this.pasteButton = paste;

        ide.element[0].addEventListener('selectionChanged', async (e) => {
            this.syncButtons();
        });

        this.syncButtons();
    }
    copyComponent() {
        if (!this.isComponentSelected)
            return;

        let component = document.querySelector('.dev-explorer-node-content.active').parentElement.getAttribute('data-id');
        let state = {
            component: component
        };

        this.updateState(state);
    }
    pasteComponent() {
        let selection = document.querySelector('.dev-explorer-node-content.active');
        let container = selection.parentElement.getAttribute('data-container');
        let folder = null;

        if (container === 'true' && selection.parentElement.getAttribute('data-static') !== 'true')
            folder = selection.parentElement.getAttribute('data-id');

        ide.ideAction({
            data: {
                'action': 'clone',
                'component': this.state.component,
                'folder': folder
            },
            onComplete: (e) => {
                tompit.success('Component pasted from the clipboard.', 'Paste component');
            }
        });
    }
    syncButtons() {
        this.copyButton.disabled = !this.isComponentSelected;
        this.pasteButton.disabled = !this.clipboardComponent || this.isComponentSelected;
    }
    loadState() {
        var url = `${tompit.DEVDEFAULTS.rootUrl}/sys/select-user-state`;
        
        this._state = tompit.post({
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: {
                primaryKey: 'env',
                topic: 'state'
            },
            url: url,
            onSuccess: (e) => {
                this.state = e;
                this.syncButtons();
            }
        });
    }
    updateState(e) {
        this.state = e;

        var url = `${tompit.DEVDEFAULTS.rootUrl}/sys/update-user-state`;

        this._state = tompit.post({
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: {
                primaryKey: 'env',
                topic: 'state',
                value: e
            },
            url: url,
            onSuccess: (e) => {
                tompit.success('Component copied to the clipboard.', 'Copy component');
            }
        });
    }
    get clipboardComponent() {
        if (!this.state)
            return null;

        return this.state.component;
    }
    get isFolderSelected() {
        let selection = document.querySelector('.dev-explorer-node-content.active');

        if (!selection)
            return false;

        let container = selection.parentElement.getAttribute('data-container');

        return container === 'true';
    }
    get isComponentSelected() {
        if (this.isFolderSelected)
            return false;

        let selection = document.querySelector('.dev-explorer-node-content.active');

        if (!selection)
            return false;

        let isStatic = selection.parentElement.getAttribute('data-static');

        return isStatic === 'false';

    }
    get copyButton() {
        return this._copyButton;
    }
    set copyButton(value) {
        this._copyButton = value;
    }
    get pasteButton() {
        return this._pasteButton;
    }
    set pasteButton(value) {
        this._pasteButton = value;
    }
    get state() {
        return this._state;
    }
    set state(value) {
        this._state = value;
    }
}

window.env = window.env || new environment();
