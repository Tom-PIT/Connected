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
            command: command,
            target: e.target
        });
    }
    dispatch(e) {
        for (let i = 0; i<e.target.parentNode.children.length; i++){
            var element = e.target.parentNode.children[i];

            if (element === e.target)
                element.classList.add('active');
            else
                element.classList.remove('active');
        }

        if (e.command === 'view-changes') {
            this.host.explorer.reload({
                provider: 'changes'
            });
        }
        else if (e.command === 'view-push') {
            this.host.explorer.reload({
                provider: 'push'
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
    commit(e) {
        const url = `${this.rootUrl}/sys/version-control/commit`;

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
            id: e.id
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
        this._domProvider = null;
        this._initialize();
        this.reload({
            provider:'changes'
        });
    }
    get domProvider() {
        return this._domProvider;
    }
    get host() {
        return this._host;
    }
    get explorer() {
        return this.host.element.querySelector('[name="explorer"] [name="explorerContent"]');
    }
    get toolbar() {
        return this.host.element.querySelector('[name="explorer"] [name="explorerToolbar"]');
    }
    get selectedItems() {
        return this._domProvider.selectedItems;
    }
    _initialize() {
        var instance = this;

        this.explorer.addEventListener('click', (e) => {
            e.stopPropagation();
            e.preventDefault();

            var caret = e.target.closest('[data-type="caret"]');

            if (caret) {
                instance.toggle({
                    element:caret
                });

                return;
            }

            var target = e.target.closest('li[data-id][data-type]');

            if (target === null)
                return;

            instance.selectNode({
                node: target,
                target:e.target
            });
        });
    }
    selectNode(e) {
        var ev = new CustomEvent('selectionChanged', {
            detail: {
                node: e.node,
                target: e.target
            },
            cancelable:true
        });

        this.host.element.dispatchEvent(ev);

        if (ev.defaultPrevented)
            return;
        
        var nodes = this.explorer.querySelectorAll('li.tp-change-item');

        for (let i = 0; i < nodes.length; i++) {
            var node = nodes[i];

            if (node === e.node)
                node.classList.add('tp-active');
            else
                node.classList.remove('tp-active');
        }
    }
    async reload(e) {
        if (e.provider === 'changes') {
            this._domProvider = new ChangesDomProvider({
                host: this.host
            });
        }
        else if (e.provider === 'push') {
            this._domProvider = new PushDomProvider({
                host: this.host
            });
        }

        this.domProvider.reload(e);
    }
    toggle(e) {
        let item = e.element.closest('li');

        e.element.classList.toggle('fa-caret-right');
        e.element.classList.toggle('fa-caret-down');

        let list = item.querySelector('li > ul');

        if (list === null)
            return;

        list.classList.toggle('tp-hidden');
    }
}

class DomProvider{
    constructor(e) {
        this._host = e.host;
        let instance = this;

        this.host.element.addEventListener('selectionChanged', e => {
            instance.onSelectionChanged(e);
        });
    }
    get selectedItems() {
        return null;
    }
    get host() {
        return this._host;
    }
    onSelectionChanged(e) {
        
    }
    reload(e) {

    }
    findNode(e) {
        return this.host.explorer.explorer.querySelector(`[data-id="${e.id}"]`);
    }
    ensureListHost(e) {
        for (let i = 0; i < e.element.children.length; i++) {
            let child = e.element.children[i];

            if (child.tagName === 'UL')
                return child;
        }

        let result = this.createList();

        e.element.appendChild(result);

        return result;
    }
    isGuidEmpty(e) {
        if (!e)
            return true;

        return e.replace(/-/g, '').replace(/0/g, '').length === 0;
    }
    createList() {
        var result = document.createElement('ul');

        result.className = 'tp-vc-list';

        return result;
    }
    createText(e) {
        let element = document.createElement('span');

        element.className = 'tp-vc-item-text';
        element.innerHTML = e.text;

        return element;
    }
    createCaret() {
        var element = document.createElement('i');

        element.className = 'fal fa-fw fa-caret-down';
        element.setAttribute('data-type', 'caret');

        return element;
    }
    createIcon(e) {
        var element = document.createElement('i');

        let scheme = 'fal';

        if (e.scheme) {
            if (e.scheme === 'solid')
                scheme = 'fas';
        }

        element.className = `${scheme} fa-fw ${e.icon} text-${e.color}`;

        return element;
    }
}

class ChangesDomProvider extends DomProvider{
    constructor(e) {
        super(e);
    }
    onSelectionChanged(e) {
        let node = e.detail.node;

        if (e.detail.target.closest('.dx-checkbox-container') !== null)
            e.preventDefault();
        else {
            var component = node.closest('li[data-type="component"]');
            var microService = node.closest('li[data-type="microService"]');

            let args = {
                id: node.getAttribute('data-id'),
                type: node.getAttribute('data-type'),
                component: component ? component.getAttribute('data-id') : null,
                microservice: microService ? microService.getAttribute('data-id') : null
            };

            this.host.diff.load(args);
        }
    }
    get selectedItems() {
        var spanElements = this.host.explorer.explorer.querySelectorAll('span[data-type="component-selection"]');
        var result = [];

        for (let i = 0; i < spanElements.length; i++) {
            let element = spanElements[i];
            var checkBox = $(element).dxCheckBox('instance');

            if (checkBox.option('value') === true) {
                let component = element.closest('[data-type="component"]');
                let microService = component.closest('[data-type="microService"]');

                result.push({
                    component: component.getAttribute('data-id'),
                    microService: microService.getAttribute('data-id')
                });
            }
        }

        return result;
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
            : this.host.explorer.explorer;

        node.innerHTML = '';
        this.host.explorer.toolbar.innerHTML = '';
        this._createToolbar();

        var list = this.createList();

        list.classList.add('tp-top-level');

        node.appendChild(list);

        for (let i = 0; i < args.changes.microServices.length; i++) {
            const change = args.changes.microServices[i];
            this._createMicroServiceElement({
                list: list,
                microService: change,
                change: change
            });
        }
    }
    _createToolbar() {
        let group = document.createElement('div');

        group.className = 'btn-group btn-group-sm';

        let commit = document.createElement('a');

        commit.setAttribute('href', '#');
        commit.innerHTML = 'Commit';

        let instance = this;

        commit.addEventListener('click', e => {
            instance.host.designer.load({
                name:'Commit'
            });  
        });

        group.appendChild(commit);

        this.host.explorer.toolbar.appendChild(group);
    }
    _createMicroServiceElement(e) {
        var item = document.createElement('li');

        item.setAttribute('data-id', e.change.id);
        item.setAttribute('data-type', 'microService');
        item.className = 'tp-change-item';
        item.append(this.createCaret());
        item.append(this.createIcon({
            icon: 'fa-share-alt',
            color: 'default'
        }));
        item.append(this.createText({
            text: e.change.name
        }));

        e.list.appendChild(item);

        var childList = this.createList();

        item.appendChild(childList);

        for (var i = 0; i < e.change.components.length; i++) {
            let component = e.change.components[i];
            let folder = this._findFolder({
                microService: e.change,
                folder: component.folder
            });

            if (!folder)
                folder = item;

            this._createComponentElement({
                folder: folder,
                component: component
            });
        }
    }
    _createComponentElement(e) {
        let listHost = this.ensureListHost({
            element: e.folder
        });

        let listItem = document.createElement('li');

        listItem.setAttribute('data-id', e.component.id);
        listItem.setAttribute('data-type', 'component');
        listItem.className = 'tp-change-item';

        if (e.component.elements && e.component.elements.length > 0)
            listItem.append(this.createCaret());

        listItem.append(this.createIcon({
            icon: 'fa-file',
            color: 'default'
        }));

        let span = document.createElement('span');

        span.className = 'px-1';
        span.setAttribute('data-type', 'component-selection');

        listItem.append(span);

        $(span).dxCheckBox({
            value:true
        });

        listItem.append(this.createText({
            text: e.component.name
        }));

        listHost.appendChild(listItem);

        this._createComponentChildren({
            item: listItem,
            items: e.component.elements
        });
    }
    _createComponentChildren(e) {
        if (!e.items || e.items.length === 0)
            return;

        var listHost = this.ensureListHost({
            element: e.item
        });

        for (let i = 0; i < e.items.length; i++) {
            let item = e.items[i];
            let listItem = document.createElement('li');

            listItem.setAttribute('data-id', item.id);
            listItem.setAttribute('data-type', 'element');
            listItem.className = 'tp-change-item';

            if (item.elements && item.elements.length > 0) {
                listItem.append(this.createCaret());

                listItem.append(this.createIcon({
                    icon: 'fa-folder',
                    color: 'warning'
                }));
            }
            else {
                listItem.append(this.createIcon({
                    icon: 'fa-file',
                    color: 'default'
                }));
            }

            let fileName = item.name;

            if (item.blob && item.blob.fileName)
                fileName = item.blob.fileName;

            listItem.append(this.createText({
                text: fileName
            }));

            listHost.appendChild(listItem);

            this._createComponentChildren({
                item: listItem,
                items: item.elements
            });
        }
    }
    _findFolder(e) {
        if (this.isGuidEmpty(e.folder))
            return null;

        let existing = this.selectFolder(e.folder);

        if (existing !== null)
            return existing;

        let stack = [];
        let current = null;

        for (let i = 0; i < e.microService.folders.length; i++) {
            let folder = e.microService.folders[i];

            if (folder.id === e.folder) {
                current = folder;
                break;
            }
        }

        while (current !== null) {
            stack.push(current);

            if (this.isGuidEmpty(current.parent))
                break;

            for (let i = 0; i < e.microService.folders.length; i++) {
                var folder = e.microService.folders[i];

                if (folder.id === current.parent) {
                    current = folder;
                    break;
                }
            }
        }

        stack = stack.reverse();
        let currentHost = this.selectMicroService(e.microService.id);

        for (let i = 0; i < stack.length; i++) {
            let folder = stack[i];
            existing = this.selectFolder(folder.id);

            if (existing === null) {
                let list = this.ensureListHost({
                    element: currentHost
                });

                let listItem = document.createElement('li');

                listItem.setAttribute('data-id', folder.id);
                listItem.setAttribute('data-type', 'folder');
                listItem.className = 'tp-change-item';

                listItem.append(this.createCaret());
                listItem.append(this.createIcon({
                    icon: 'fa-folder',
                    scheme: 'solid',
                    color: 'warning'
                }));

                listItem.append(this.createText({
                    text: folder.name
                }));

                list.appendChild(listItem);

                currentHost = listItem;
            }
        }

        return currentHost;
    }
    selectFolder(id) {
        return this.host.explorer.explorer.querySelector(`li[data-type="folder"][data-id="${id}"]`);
    }
    selectMicroService(id) {
        return this.host.explorer.explorer.querySelector(`li[data-type="microService"][data-id="${id}"]`);
    }

}

class PushDomProvider extends DomProvider{
    constructor(e) {
        super(e);
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
            : this.host.explorer.explorer;

        node.innerHTML = '';
        this.host.explorer.toolbar.innerHTML = '';
    }
}