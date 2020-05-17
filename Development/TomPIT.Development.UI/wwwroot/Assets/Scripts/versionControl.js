export class VersionControl {
    constructor(e) {
        this.selector = e.selector;

        const args = {
            element: this,
            arguments: e
        };

        this._toolbar = new Toolbar(args);
        this._designer = new Designer(args);
        this._server = new Server(args);
        this._diff = new Diff(args);
        this._explorer = new Explorer(args);
    }
    get designer() {
        return this._designer;
    }
    get toolbar() {
        return this._toolbar;
    }
    get server() {
        return this._server;
    }
    get element() {
        return document.querySelector(this.selector);
    }
    get explorer() {
        return this._explorer;
    }
    get diff() {
        return this._diff;
    }
}

class Toolbar {
    constructor(e) {
        this._host = e.element;

        this.initialize();
    }
    get host() {
        return this._host;
    }
    get toolbar() {
        return this.host.element.querySelector('[name="designerToolbar"]');
    }
    get toolbarButtons() {
        var links = this.toolbar.querySelectorAll('[role="button"]');
        var result = [];

        for (let i = 0; i < links.length; i++)
            result.push(links[i]);

        return result;
    }
    initialize() {
        var buttons = this.toolbarButtons;

        for (let i = 0; i < buttons.length; i++)
            buttons[i].addEventListener('click', this.toolbarButtonClick.bind(this));
    }
    toolbarButtonClick(e) {
        e.stopPropagation();
        e.preventDefault();

        var command = e.currentTarget.getAttribute('data-command');

        this.dispatch({
            command: command
        });
    }
    dispatch(e) {
        if (e.command === 'repositories') {
            this.host.designer.load({
                name: 'repositories'
            });         
        }
    }
}

class Designer {
    constructor(e) {
        this._host = e.element;
    }
    get host() {
        return this._host;
    }
    get designer() {
        return this.host.element.querySelector('[name="designer"]');
    }
    show() {
        this.designer.classList.add('tp-active');
    }
    hide() {
        this.designer.classList.remove('tp-active');
    }
    async load(e) {
        let partial = await this.host.server.designer(e);
        this.designer.setHtml(partial);
        this.show();
        this.host.diff.hide();
    }
}

class Server {
    constructor(e) {
        this._rootUrl = e.arguments.rootUrl;
    }
    get rootUrl() {
        return this._rootUrl;
    }
    designer(e) {
        const url = `${this.rootUrl}/sys/version-control/designer`;

        return new Promise((resolve) => {
            tompit.post({
                url: url,
                data: e,
                onSuccess: async (e) => {
                    resolve(e);
                }
            });
        });
    }
    changes(e) {
        const url = `${this.rootUrl}/sys/version-control/changes`;

        return new Promise((resolve) => {
            tompit.post({
                url: url,
                data: e,
                onSuccess: async (e) => {
                    resolve(e);
                }
            });
        });
    }
    diff(e) {
        const url = `${this.rootUrl}/sys/version-control/diff`;

        return new Promise((resolve) => {
            tompit.post({
                url: url,
                data: e,
                onSuccess: async (e) => {
                    resolve(e);
                }
            });
        });
    }
}

HTMLElement.prototype.setHtml = function (html) {
    this.innerHTML = html;

    let scripts = this.querySelectorAll('script');

    for (let i = 0; i < scripts.length; i++) {
        const script = scripts[i];

        if (script.getAttribute('src') !== null)
            continue;

        let scriptElement = document.createElement('script');

        for (let j = 0; j < script.attributes.length; j++) {
            const attribute = script.attributes[j];

            scriptElement.setAttribute(attribute.name, attribute.value);
        }

        scriptElement.innerHTML = script.innerHTML;
        var sibling = script.nextSibling;
        script.remove();

        if (sibling === null)
            this.append(scriptElement);
        else
            this.insertBefore(scriptElement, sibling);
    }
};

class Diff {
    constructor(e) {
        this._host = e.element;

        this._createEditor();
        this._serverModel = null;
        this._localModel = null;
        this._editor = null;
    }
    get host() {
        return this._host;
    }
    get diff() {
        return this._host.element.querySelector('[name="diff"]');
    }
    get serverModel() {
        return this._serverModel;
    }
    set serverModel(value) {
        this._serverModel = value;
    }
    get localModel() {
        return this._localModel;
    }
    set localModel(value) {
        this._localModel = value;
    }
    get editor() {
        return this._editor;
    }
    async load(e) {
        this.show();
        this.host.designer.hide();

        var result = await this.host.server.diff({
            component: e.component,
            blob: e.blob
        });

        this.serverModel = monaco.editor.createModel(result.original, result.syntax);
        this.localModel = monaco.editor.createModel(result.modified, result.syntax);

        this._editor.setModel({
            modified: this.localModel,
            original: this.serverModel
        });
    }
    show() {
        this.diff.classList.add('tp-active');

        if (this.editor)
            this._editor.layout();
    }
    hide() {
        this.diff.classList.remove('tp-active');
    }
    _createEditor() {
        var instance = this;

        require(['vs/editor/editor.main'], function () {
            instance._setTheme();
            let options = {

            };

            instance._editor = monaco.editor.createDiffEditor(instance.diff, options);
            instance._editor.layout();
        });
    }
    _setTheme() {
        monaco.editor.defineTheme('TomPIT', {
            base: 'vs',
            inherit: true,
            rules: [{ background: 'f5f5f5' }]
        });

        monaco.editor.setTheme('TomPIT');
    }
}

class Explorer {
    constructor(e) {
        this._host = e.element;

        this._initialize();
        this.reload();
    }
    get host() {
        return this._host;
    }
    get explorer() {
        return this.host.element.querySelector('[name="explorer"]');
    }
    _initialize() {
        var instance = this;

        this.explorer.addEventListener('click', (e) => {
            e.stopPropagation();
            e.preventDefault();

            var target = e.target.closest('li[data-id][data-type]');

            if (target === null)
                return;

            instance.selectNode({
                id: target.getAttribute('data-id'),
                type: target.getAttribute('data-type'),
                component: target.getAttribute('data-component'),
                microservice: target.getAttribute('data-microservice'),
                folder: target.getAttribute('data-folder'),
                blob: target.getAttribute('data-blob')
            });
        });
    }
    selectNode(e) {
        var nodes = this.explorer.querySelectorAll('li.tp-change-item');
        var target = this.explorer.querySelector(`li.tp-change-item[data-id="${e.id}"]`);

        var isBlob = e.id.replace('-', '').replace('0', '').trim().length > 0;

        if (isBlob)
            this.host.diff.load(e);
        else
            this.host.designer.load(e);

        for (let i = 0; i < nodes.length; i++) {
            var node = nodes[i];

            if (node === target)
                node.classList.add('tp-active');
            else
                node.classList.remove('tp-active');
        }
    }
    findNode(e) {
        return this.explorer.querySelector(`[data-id="${e.id}"]`);
    }
    async reload(e) {
        let id = null;

        if (e && e.id)
            id = e.id;

        var args = {
            id: id
        };

        args.changes = await this.host.server.changes(args);

        var node = id
            ? this.findNode(args)
            : this.explorer;

        node.innerHTML = '';

        var list = document.createElement('ul');

        node.appendChild(list);

        for (let i = 0; i < args.changes.length; i++) {
            const change = args.changes[i];
            var item = this._createElement({
                list: list,
                change: change
            });

            this._loadChildren({
                item: item,
                change: change
            });
        }
    }
    _loadChildren(e) {
        if (!e.change.items)
            return;

        var list = document.createElement('ul');

        for (let i = 0; i < e.change.items.length; i++) {
            const change = e.change.items[i];
            var item = this._createElement({
                list: list,
                change: change
            });

            this._loadChildren({
                item: item,
                change: change
            });
        }

        e.item.appendChild(list);
    }
    _createElement(e) {
        let item = document.createElement('li');

        item.setAttribute('data-id', e.change.id);
        item.setAttribute('data-type', e.change.type);
        item.setAttribute('data-component', e.change.component);
        item.setAttribute('data-microservice', e.change.microservice);
        item.setAttribute('data-folder', e.change.folder);
        item.setAttribute('data-blob', e.change.blob);
        item.className = 'tp-change-item';
        item.innerHTML = e.change.name;

        e.list.appendChild(item);

        return item;
    }
}