(function ($) {
    'use strict';

    $.widget('tompit.tpTextEditor', {
        options: {
            instance: null,
            state: [],
            languages: []
        },
        _create: function () {
            var target = this;

            require(['vs/editor/editor.main'], function () {
                monaco.editor.defineTheme('TomPIT', {
                    base: 'vs',
                    inherit: true,
                    rules: [{ background: 'f5f5f5' }]
                });

                monaco.editor.setTheme('TomPIT');


                var editor = $('[data-role="text-editor"]', $(target.element));
                if (!editor.length) {
                    console.warn('Editor container not found!!!');
                }

                var editorOpts = {
                    language: null,
                    value: null,
                    lineNumbers: true,
                    scrollBeyondLastLine: true,
                    automaticLayout: true,
                    folding: true,
                    formatOnPaste: true,
                    formatOnType: true,
                    glyphMargin: true,
                    lineNumbersMinChars: 4,
                    parameterHints: {
                        enabled: true
                    },
                    wrappingColumn: 0,
                    wrappingIndent: 'indent',
                    showUnused: true,
                    autoIndent: true,
                    wordWrap: 'off',
                    tabCompletion: 'on',
                    model: null,
                    minimap: {
                        enabled: false
                    }
                };

                if (target.options.language) {
                    editorOpts.language = target.options.language;
                }

                if (target.options.source) {
                    editorOpts.value = target.options.source.join('\n');
                    editorOpts.model = undefined;
                }

                target.options.instance = monaco.editor.create(
                    editor[0],
                    editorOpts,
                    {
                        editorService: {
                            openEditor: function (e) {
                                alert(`open editor called!` + JSON.stringify(e));
                            },
                            resolveEditor: function (e) {
                                alert(`open editor called!` + JSON.stringify(e));
                            }
                        }
                    }
                );

                target.options.instance.setup = function (options) {
                    var debug = options.debug;
                    var debugButton = $('#btnDebug');

                    $('#customActions').empty();

                    if (options.actions) {
                        for (let i = 0; i < options.actions.length; i++) {
                            const action = options.actions[i];
                            $('#customActions').append(`<a href="#" class="btn btn-sm btn-light" title="${action.text}" data-tag="custom-action" data-action="${action.action}"><i class="${action.glyph}"></i></a>`);
                        }

                        $('a[data-tag="custom-action"]').click((e) => {
                            var target = $(e.target).closest('a');

                            ide.designerAction({
                                data: {
                                    action: target.attr('data-action')
                                },
                                onComplete: () => {
                                    tompit.success('Entity synchronized');
                                }
                            });
                        });
                    }

                    if (typeof debug !== 'undefined' && typeof debug.url !== 'undefined' && debug.url.length > 0) {
                        debugButton.collapse('show');
                        debugButton.attr('href', debug.url);
                    }
                    else
                        debugButton.collapse('hide');

                    if (typeof options.language === 'undefined' || options.length === 0)
                        $('#labelSyntaxLanguage').collapse('hide');
                    else {
                        $('#labelSyntaxLanguage').html(options.language);
                        $('#labelSyntaxLanguage').collapse('show');
                    }
                };


                monaco.editor.onDidCreateModel((model) => {
                    model.onDidChangeContent((e) => {
                        try {
                            target.setState({
                                model: model.uri.toString(),
                                dirty: true,
                                timestamp: Date.now()
                            });
                        }
                        catch (e) {
                            console.log(e);
                        }
                    });
                });

                if ($.isFunction(target.options.onCreated)) {
                    target.options.onCreated(target, target.getInstance());
                }
            });
        },

        getDirtyModels: function () {
            var result = [];

            $.each(this.options.state, function (i, v) {
                if (v.dirty)
                    result.push(v);
            });

            return result;
        },
        setState: function (state) {
            var existingState = null;
            var existingStateIndex = -1;

            $.each(this.options.state, function (i, v) {
                if (v.model === state.model) {
                    if (!state.dirty) {
                        if (state.timestamp < v.timestamp)
                            return false;
                    }

                    existingState = v;
                    existingStateIndex = i;
                    return false;
                }
            });

            if (existingState === null) {
                if (!state.dirty)
                    return;

                if (typeof state.timestamp === 'undefined')
                    state.timestamp = Date.now();

                this.options.state.push(state);
            }
            else {
                if (!state.dirty && state.timestamp >= existingState.timestamp)
                    this.options.state.splice(existingStateIndex, 1);
                else
                    existingState.dirty = true;
            }
        },
        getInstance: function () {
            return this.options.instance;
        },
        isLanguageInitialized: function (language) {
            var result = false;

            $.each(this.options.languages, function (i, v) {
                if (v === language) {
                    result = true;
                    return false;
                }
            });

            return result;
        },
        initializeLanguage: function (language, features) {
            this.options.languages.push(language);

            if (features.codeAction)
                this._codeAction(language);

            if (features.completionItem)
                this._completionItem(language);

            if (features.declaration)
                this._declaration(language);

            if (features.signatureHelp)
                this._signatureHelp(language);

            if (features.documentSymbol)
                this._documentSymbol(language);

        },
        setTargetProperty: function (property) {
            this.options.property = property;
        },
        _codeAction: function (language) {
            var instance = this;

            monaco.languages.registerCodeActionProvider(language, {
                provideCodeActions: function (model, range, context) {
                    return new Promise(function (resolve, reject) {
                        try {
                            ide.designerAction({
                                data: {
                                    action: 'provideCodeActions',
                                    section: 'designer',
                                    property: instance.options.property,
                                    model: {
                                        'id': model.id,
                                        'uri': model.uri.toString(),
                                        'version': model._versionId
                                    },
                                    range: range,
                                    context: context,
                                    text: model.getValue()
                                },
                                onComplete: function (data) {
                                    if (data) {
                                        for (let i = 0; i < data.length; i++) {
                                            var edit = data[i].edit;

                                            if (edit) {
                                                for (let j = 0; j < edit.edits.length; j++) {
                                                    var textEdit = edit.edits[j];

                                                    if (textEdit.resource) {
                                                        textEdit.resource = monaco.Uri.parse(textEdit.resource);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    resolve({
                                        actions: data === null ? [] : data,
                                        dispose: function () { data = null; }
                                    });
                                }

                            }, false);
                        }
                        catch (e) {
                            reject();
                            console.log(e);
                        }
                    });
                }
            });
        },
        _completionItem: function (language) {
            var instance = this;

            monaco.languages.registerCompletionItemProvider(language, {
                provideCompletionItems: function (model, position, context) {
                    return new Promise(function (resolve, reject) {
                        try {
                            ide.designerAction({
                                data: {
                                    action: 'provideCompletionItems',
                                    section: 'designer',
                                    property: instance.options.property,
                                    model: {
                                        'id': model.id,
                                        'uri': model.uri.toString(),
                                        'version': model._versionId
                                    },
                                    position: position,
                                    context: context,
                                    text: model.getValue()
                                },
                                onComplete: function (data) {
                                    if (data) {
                                        data.dispose = function () {
                                            data = null;
                                        };

                                        resolve(data);
                                    }
                                    else
                                        reject();
                                }

                            }, false);
                        }
                        catch (e) {
                            reject();
                            console.log(e);
                        }
                    });
                }
            });
        },
        _declaration: function (language) {
            var instance = this;

            monaco.languages.registerDeclarationProvider(language, {
                provideDeclaration: function (model, position) {
                    return new Promise(function (resolve, reject) {
                        try {
                            ide.designerAction({
                                data: {
                                    action: 'provideDeclaration',
                                    section: 'designer',
                                    property: instance.options.property,
                                    model: {
                                        'id': model.id,
                                        'uri': model.uri.toString(),
                                        'version': model._versionId
                                    },
                                    position: position,
                                    text: model.getValue()
                                },
                                onComplete: function (data) {
                                    resolve({
                                        uri: monaco.Uri.parse(data.uri),
                                        range: data.range
                                    });
                                }

                            }, false);
                        }
                        catch (e) {
                            reject();
                            console.log(e);
                        }
                    });
                }
            });
        },
        _signatureHelp: function (language) {
            var instance = this;

            monaco.languages.registerSignatureHelpProvider(language, {
                provideSignatureHelp: function (model, position, token, context) {
                    return new Promise(function (resolve, reject) {
                        try {
                            ide.designerAction({
                                data: {
                                    action: 'provideSignatureHelp',
                                    section: 'designer',
                                    property: instance.options.property,
                                    model: {
                                        'id': model.id,
                                        'uri': model.uri.toString(),
                                        'version': model._versionId
                                    },
                                    position: position,
                                    text: model.getValue()
                                },
                                onComplete: function (data) {
                                    if (data) {
                                        resolve({
                                            value: data,
                                            dispose: function () {

                                                data = null;
                                            }
                                        });
                                    }
                                    else
                                        reject();
                                }

                            }, false);
                        }
                        catch (e) {
                            reject();
                            console.log(e);
                        }
                    });
                }
            });
        },
        _documentSymbol: function (language) {
            var instance = this;

            monaco.languages.registerDocumentSymbolProvider(language, {
                provideDocumentSymbols: function (model, position, token, context) {
                    return new Promise(function (resolve, reject) {
                        try {
                            ide.designerAction({
                                data: {
                                    action: 'provideDocumentSymbols',
                                    section: 'designer',
                                    property: instance.options.property,
                                    model: {
                                        'id': model.id,
                                        'uri': model.uri.toString(),
                                        'version': model._versionId
                                    },
                                    text: model.getValue()
                                },
                                onComplete: function (data) {
                                    if (data)
                                        resolve(data);
                                    else
                                        reject();
                                }

                            }, false);
                        }
                        catch (e) {
                            reject();
                            console.log(e);
                        }
                    });
                }
            });
        }
    });
})(jQuery);