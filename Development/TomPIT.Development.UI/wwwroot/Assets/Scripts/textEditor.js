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

                target.options.instance = monaco.editor.create(document.getElementById('devTextDesignerEditor'), {
                    language: null,
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
                    tabCompletion:'on',
                    model: null,
                    minimap: {
                        enabled: false
                    }
                },
                    {
                        editorService: {
                            openEditor: function(e) {
                                alert(`open editor called!` + JSON.stringify(e));
                            },
                            resolveEditor:function(e) {
                                alert(`open editor called!` + JSON.stringify(e));
                            }
                        }
                    });

                target.options.instance.setup = function (options) {
                    var debug = options.debug;
                    var debugButton = $('#btnDebug');

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

                monaco.editor.onDidCreateModel((model)=>{
                    model.onDidChangeContent((e) => {
                        try {
                            target.setState({
                                model: model.uri.toString(),
                                dirty: true
                            });
                        }
                        catch (e) {
                            console.log(e);
                        }
                    });
                });

                if ($.isFunction(target.options.onCreated))
                    target.options.onCreated(target, target.getInstance());
            });
        },

        getDirtyModels: function () {
            var result = [];

            $.each(this.options.state, function (i, v) {
                if (v.dirty)
                    result.push(v.model);
            });

            return result;
        },
        setState: function (state) {
            var existingState = null;
            var existingStateIndex = -1;

            $.each(this.options.state, function (i, v) {
                if (v.model === state.model) {
                    existingState = v;
                    existingStateIndex = i;
                    return false;
                }
            });

            if (existingState === null) {
                if (!state.dirty)
                    return;

                this.options.state.push(state);
            }
            else {
                if (!state.dirty)
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
                                        'uri': model.uri.toString()
                                    },
                                    range: range,
                                    context: context,
                                    text: model.getValue()
                                },
                                onComplete: function (data) {
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
                                        'uri': model.uri.toString()
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
                                        'uri': model.uri.toString()
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
                                        'uri': model.uri.toString()
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
                                        'uri': model.uri.toString()
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