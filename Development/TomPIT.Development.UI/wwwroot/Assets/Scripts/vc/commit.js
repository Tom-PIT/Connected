export class Commit {
    constructor(e) {
        this._selector = e.selector;
        this._setupUI();
    }

    get element() {
        return document.querySelector(this._selector);
    }
    _setupUI() {
        $('#editComment').dxTextArea({
            valueChangeEvent:'keyup',
            onValueChanged: e => {
                let button = document.querySelector('#btnCommit');
                
                button.disabled = !e.value ? true : false;
            }
        });

        this.element.querySelector('#btnCommit').addEventListener('click', async e => {
            let selectedComponents = versionControl.explorer.selectedItems;

            await versionControl.server.commit({
                components: selectedComponents,
                comment: $('#editComment').dxTextArea('instance').option('value')
            });

            await versionControl.explorer.reload({
                provider: 'changes'
            });

            versionControl.explorer.selectNode({
                node: null,
                target: null
            });
        });
    }
}