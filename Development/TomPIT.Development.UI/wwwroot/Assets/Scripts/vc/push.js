export class Push {
    constructor(e) {
        this._dataSources = e.dataSources;
        this._selector = e.selector;
        this._setupUI();
    }

    get element() {
        return document.querySelector(this._selector);
    }
    _setupUI() {
        var instance = this;

        $('#editComment').dxTextArea({
            valueChangeEvent:'keyup',
            onValueChanged: e => {
                let button = document.querySelector('#btnPush');
                
                button.disabled = !e.value ? true : false;
            }
        });

        this.element.querySelector('#btnPush').addEventListener('click', async e => {
            let selectedComponents = versionControl.explorer.selectedItems;

            await versionControl.server.push({
                components: selectedComponents,
                comment: $('#editComment').dxTextArea('instance').option('value')
            });

            await versionControl.explorer.reload({
                provider: 'push'
            });

            versionControl.explorer.selectNode({
                node: null,
                target: null
            });
        });

        $('#editBinding').dxSelectBox({
            dataSource: this._dataSources.bindings,
            displayExpr: 'name',
            valueExpr: 'id',
            onValueChanged: async e => {
                instance.reloadBranches({
                    repository: e.value
                });
            }
        });
    }
    async reloadBranches(e) {
        var branches = await versionControl.server.queryBranches({
            repository : e.repository
        });
    }
}