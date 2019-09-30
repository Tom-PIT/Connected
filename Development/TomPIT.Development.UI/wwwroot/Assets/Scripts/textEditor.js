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
                    wordWrap: 'on',
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

        },
        setTargetProperty: function (property) {
            this.options.property = property;
        },
        _codeAction: function (language) {
            var instance = this;

            monaco.languages.registerCodeActionProvider(language, {
                provideCodeActions: function (model, range, context) {
                    return new Promise(function (resolve, reject) {
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
                                    actions: data,
                                    dispose: function () { }
                                });
                            }

                        }, false);
                    });
                }
            });
        },
        _completionItem: function (language) {
            var instance = this;

            monaco.languages.registerCompletionItemProvider(language, {
                provideCompletionItems: function (model, position, context) {
                    return new Promise(function (resolve, reject) {
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
                                    data.dispose = function () { };

                                    resolve(data);
                                }
                                else
                                    reject();
                            }

                        }, false);
                    });
                }
            });
        },
        _declaration: function (language) {
            var instance = this;

            monaco.languages.registerDeclarationProvider(language, {
                provideDeclaration: function (model, position) {
                    return new Promise(function (resolve, reject) {
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
                                    range:data.range
                                });
                            }

                        }, false);
                    });
                }
            });
        },
        _signatureHelp: function (language) {
            var instance = this;

            monaco.languages.registerSignatureHelpProvider(language, {
                provideSignatureHelp: function (model, position, token, context) {
                    return new Promise(function (resolve, reject) {
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
                                        dispose: function () {}
                                    });
                                }
                                else
                                    reject();
                            }

                        }, false);
                    });
                }
            });
        }
    });

    $.widget('tompit.tpTextEditor1', {

        options: {
            onCreated: function (instance) {
                return false;
            },
            onChanged: function (value) {
                return false;
            },
            instance: null
        },

        _create: function () {
            this._editors = [];
        },
        deactivateEditor: function () {
            var target = this;

            if (typeof target.options.instance !== 'undefined' && target.options.instance !== null) {
                try {
                    if (typeof target.options.timer !== 'undefined')
                        clearInterval(target.options.timer);

                    if (typeof target.options.instance.completionItemProvider !== 'undefined')
                        target.options.instance.completionItemProvider.dispose();

                    if (typeof target.options.instance.signatureHelpProvider !== 'undefined')
                        target.options.instance.signatureHelpProvider.dispose();

                    if (typeof target.options.instance.hoverProvider !== 'undefined')
                        target.options.instance.hoverProvider.dispose();

                    if (typeof target.options.instance.codeLensProvider !== 'undefined')
                        target.options.instance.codeLensProvider.dispose();

                    if (typeof target.options.instance.definitionProvider !== 'undefined')
                        target.options.instance.definitionProvider.dispose();

                    if (typeof target.options.instance.codeActionProvider !== 'undefined')
                        target.options.instance.codeActionProvider.dispose();

                    target.options.instance.dispose();
                    target.options.instance = null;
                }
                catch (e) {
                    console.log(e);
                }
            }
        },
        activateEditor: function (options) {
            var target = this;

            require(['vs/editor/editor.main'], function () {
                monaco.editor.defineTheme('TomPIT', {

                    base: 'vs',
                    inherit: true,
                    rules: [{ background: 'f5f5f5' }]
                });

                monaco.editor.setTheme('TomPIT');

                target.deactivateEditor();

                target.options = $.extend({
                    readOnly: false
                }, options);

                var src = options.source === null || options.source.length === 0 ? '\n' : options.source.join('\n');

                target.options.hasChanged = false;
                target.options.changeState = false;
                target.options.instance = monaco.editor.create(document.getElementById(options.elementId), {
                    value: src,
                    language: options.language,
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
                    readOnly: options.readOnly,
                    showUnused: true,
                    autoIndent: true,
                    wordWrap: 'on',
                    tabCompletion: 'on'
                });

                target.options.timer = setInterval(function () {
                    try {
                        if ($.isFunction(target.options.onCheckSyntax) && target.observeDirty())
                            target.options.onCheckSyntax();
                    } catch (e) {
                        console.log(e);
                    }
                }, 2500);

                if ($.isFunction(options.onCreated))
                    options.onCreated(target.options.instance);

                target.options.instance.onDidChangeModelContent((e) => {
                    target.options.hasChanged = true;
                    target.options.changeState = true;
                });

                $('textarea', target.element).on('blur', function () {
                    if (target.options.hasChanged && typeof options.onChange !== 'undefined') {
                        options.onChange(target.options.instance.getValue());
                        target.options.hasChanged = false;
                    }
                });
            });
        },
        registerCompletionItemProvider: function (language, options) {
            var provider = monaco.languages.registerCompletionItemProvider(language, options);

            this.options.instance.completionItemProvider = provider;
        },
        registerSignatureHelpProvider: function (language, options) {
            var provider = monaco.languages.registerSignatureHelpProvider(language, options);

            this.options.instance.signatureHelpProvider = provider;
        },
        registerHoverProvider: function (language, options) {
            var provider = monaco.languages.registerHoverProvider(language, options);

            this.options.instance.hoverProvider = provider;
        },
        registerCodeLensProvider: function (language, options) {
            var provider = monaco.languages.registerCodeLensProvider(language, options);

            this.options.instance.codeLensProvider = provider;
        },
        registerDefinitionProvider: function (language, options) {
            var provider = monaco.languages.registerDefinitionProvider(language, options);

            this.options.instance.definitionProvider = provider;
        },
        registerCodeActionProvider: function (language, options) {
            var provider = monaco.languages.registerCodeActionProvider(language, options);

            this.options.instance.codeActionProvider = provider;
        },
        insertText: function (text) {
            var line = this.options.instance.getPosition();
            var range = new monaco.Range(line.lineNumber, 1, line.lineNumber, 1);
            var identifier = { major: 1, minor: 1 };
            var op = { identifier: identifier, range: range, text: text, forceMoveMarkers: true };

            this.options.instance.executeEdits("insert-snippet", [op]);
        },
        getValue: function () {
            return this.options.instance.getValue();
        },
        setValue: function (s) {
            return this.options.instance.setValue(s);
        },
        formatDocument: function () {
            this.options.instance.trigger('format', 'editor.action.formatDocument');
        },
        setMarkers: function (markers) {
            monaco.editor.setModelMarkers(this.options.instance.getModel(), 'syntax', markers);

        },
        addCommand: function (k, h) {
            return this.options.instance.addCommand(k, h);
        },
        addAction: function (k) {
            return this.options.instance.addAction(k);
        },
        observeDirty: function () {
            var r = this.options.changeState;

            this.options.changeState = false;

            return r;
        },
        getEditor: function () {
            return this.options.instance;
        }

    });
})(jQuery);