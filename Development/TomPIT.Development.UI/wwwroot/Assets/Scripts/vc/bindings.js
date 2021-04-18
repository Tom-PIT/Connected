export class Bindings {
    constructor(e) {
        this._dataSources = e.dataSources;
        this._selector = e.selector;
        this._setupUI();
    }

    get element() {
        return document.querySelector(this._selector);
    }
    showList() {
        let form = document.querySelector('#form');
        let list = document.querySelector('#list');

        list.classList.add('show');
        form.classList.remove('show');
    }
    showEditor() {
        let form = document.querySelector('#form');
        let list = document.querySelector('#list');

        list.classList.remove('show');
        form.classList.add('show');
    }
    _setupUI() {
        document.querySelector('#btnAddBinding').addEventListener('click', async e => {
            let designer = await versionControl.server.designer({
                name:'BindingForm'
            });

            this.showEditor();
            form.setHtml(designer);
        });

        $('#gridList').dxDataGrid({
            dataSource: this._dataSources.bindings,
            keyExpr: 'name',
            paging: {
                pageSize: 10
            },
            pager: {
                showInfo: true
            },
            searchPanel: {
                visible: false
            },
            columns: [
                {
                    dataField: "name",
                    caption: "Binding",
                    sortOrder: 'asc',
                    calculateCellValue: e => {
                        return [e.name, e.url].join();
                    },
                    cellTemplate: (element, info) => {
                        var name = info.data.name;
                        var url = info.data.url;
                        
                        var container = document.createElement('div');
                        var nameLink = document.createElement('a');

                        nameLink.setAttribute('href', '#');
                        nameLink.setAttribute('data-id', name);
                        nameLink.setAttribute('data-type', 'binding');
                        nameLink.innerHTML = name;

                        container.appendChild(nameLink);

                        var urlContainer = document.createElement('div');
                        urlContainer.className = "small";

                        var urlLink = document.createElement('a');

                        urlLink.innerHTML = url;
                        urlLink.setAttribute('href', url);
                        urlContainer.appendChild(urlLink);
                        container.appendChild(urlContainer);
                        
                        element[0].appendChild(container);
                    }
                }
            ],
            onContentReady: e => {
                let bindings = document.querySelectorAll('[data-type="binding"]');

                for (let i = 0; i < bindings.length; i++) {
                    let binding = bindings[i];

                    binding.addEventListener('click', async e => {
                        let designer = await versionControl.server.designer({
                            name: 'BindingForm',
                            binding:binding.getAttribute('data-id')
                        });

                        this.showEditor();
                        form.setHtml(designer);
                    });
                }
            }
        });
    }
    initEditor(e) {
        let container = document.querySelector(e.selector);
        let cancel = container.querySelector('#btnCancel');
        let save = container.querySelector('#btnSave');
        let instance = this;

        cancel.addEventListener('click', e => {
            instance.showList();
        });

        $('#editName').dxTextBox({
            value: e.data.name,
            maxLength: 128
        }).dxValidator({
            validationGroup: 'binding',
            validationRules: [
                {
                    type:'required'
                }
            ]
        });

        $('#editUrl').dxTextBox({
            value: e.data.url,
            maxLength: 1024
        }).dxValidator({
            validationGroup: 'binding',
            validationRules: [
                {
                    type: 'required'
                }
            ]
        });

        $('#editUserName').dxTextBox({
            value: e.data.userName,
            maxLength: 128
        }).dxValidator({
            validationGroup: 'binding',
            validationRules: [
                {
                    type: 'required'
                }
            ]
        });

        $('#editPassword').dxTextBox({
            maxLength: 128,
            mode:'password'
        }).dxValidator({
            validationGroup: 'binding',
            validationRules: [
                {
                    type: 'required'
                }
            ]
        });

        save.addEventListener('click', async args => {
            if (!DevExpress.validationEngine.validateGroup('binding').isValid)
                return;

            var action = e.data.name
                ? 'updateBinding'
                : 'insertBinding';

            await versionControl.server.designerAction({
                action: action,
                existingName: e.data.name,
                name: $('#editName').dxTextBox('instance').option('value'),
                url: $('#editUrl').dxTextBox('instance').option('value'),
                userName: $('#editUserName').dxTextBox('instance').option('value'),
                password: $('#editPassword').dxTextBox('instance').option('value')
            });
        });
    }
}