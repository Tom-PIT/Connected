(function ($) {
    'use strict';

    $.widget('tompit.tpTextEditor', {
        options: {
            instance: null,
            state: [],
            languages: [],
            colors: [
                { ms: null, color: 'transparent' },
                { ms: null, color: '#e1f5fe' },
                { ms: null, color: '#e8f5e9' },
                { ms: null, color: '#fbe9e7' },
                { ms: null, color: '#ede7f6' },
                { ms: null, color: '#eceff1' },
                { ms: null, color: '#efebe9' },
                { ms: null, color: '#fff8e1' },
                { ms: null, color: '#fce4ec' },
                { ms: null, color: '#e0f7fa' },
                { ms: null, color: '#fffde7' }
            ],
            decorations: []
        },
        _create: function () {
            var target = this;

            let tabs = document.querySelector('#modelTabs');

            if (tabs !== null) {
                tabs.addEventListener('click', (e) => {
                    e.preventDefault();
                    e.stopPropagation();

                    var t = e.target.closest('[data-model]');

                    if (t === null)
                        return;

                    target.activateModel(t.getAttribute('data-model'));
                });

                tabs.addEventListener('dblclick', (e) => {
                    e.preventDefault();
                    e.stopPropagation();

                    var t = e.target.closest('[data-model]');

                    if (t === null)
                        return;

                    target.closeModel(t.getAttribute('data-model'));
                });
            }

            require(['vs/editor/editor.main'], function () {
                monaco.editor.setTheme('vs');


                var editor = $('[data-role="text-editor"]', $(target.element));
                if (!editor.length) {
                    console.warn('Editor container not found.');
                }

                target._resizeObserver = new ResizeObserver((elements) => {
                    for (var element of elements) {
                        target.getInstance().layout({ width: 0, height: 0 });

                        var rect = element.target.getBoundingClientRect();

                        if (rect.width && rect.height)
                            target.getInstance().layout({ width: Math.floor(rect.width), height: Math.floor(rect.height)});
                        else
                            target.getInstance().layout();
                    }
                });

                target.registerForObservation = (element) => target._resizeObserver.observe(element);

                var editorOpts = {
                    language: null,
                    value: null,
                    lineNumbers: true,
                    scrollBeyondLastLine: true,
                    automaticLayout: false,
                    fontSize: '14px',
                    autoClosingBrackets: 'beforeWhitespace',
                    autoClosingOvertype: 'always',
                    autoClosingQuotes: 'beforeWhitespace',
                    autoIndent: 'brackets',
                    colorDecorators: true,
                    fixedOverflowWidgets: true,
                    folding: true,
                    formatOnPaste: true,
                    formatOnType: true,
                    glyphMargin: true,
                    lineNumbersMinChars: 4,
                    layoutInfo: {
                        heigth: '100%',
                    },
                    parameterHints: {
                        enabled: true
                    },
                    wrappingColumn: 0,
                    wrappingIndent: 'indent',
                    showUnused: true,
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
                        textModelService: {
                            createModelReference: (uri) => {
                                return new Promise((resolve, reject) => {
                                    var result = {
                                        uri: uri,
                                        load: () => {
                                            return Promise.resolve(textEditorModel);
                                        },
                                        dispose: () => {

                                        },
                                        textEditorModel: null
                                    };

                                    result.textEditorModel = monaco.editor.getModel(uri);

                                    if (result.textEditorModel === null) {
                                        ide.designerAction({
                                            data: {
                                                action: 'loadModel',
                                                model: {
                                                    'uri': uri.toString()
                                                }
                                            },
                                            onComplete: (e) => {
                                                result.textEditorModel = monaco.editor.createModel(e.text, e.language, uri.toString());

                                                result.textEditorModel.fileName = e.fileName;
                                                result.textEditorModel.microService = e.microService;

                                                if (!textEditorState.isLanguageInitialized(e.language)) {
                                                    textEditorState.initializeLanguage(e.language, {
                                                        codeAction: e.codeAction,
                                                        completionItem: e.codeCompletion,
                                                        declaration: e.declaration,
                                                        definition: e.definition,
                                                        signatureHelp: e.signatureHelp,
                                                        documentSymbol: e.documentSymbol,
                                                        documentFormatting: e.documentFormatting
                                                    });
                                                }


                                                resolve({
                                                    object: result,
                                                    dispose: () => { }
                                                });
                                            }
                                        });
                                    }
                                    else {
                                        resolve({
                                            object: result,
                                            dispose: () => { }
                                        });
                                    }
                                });
                            }
                        }
                    }
                );

                const editorService = target.options.instance._codeEditorService;
                const openEditorBase = editorService.openCodeEditor.bind(editorService);

                editorService.openCodeEditor = (input, source) => {
                    return new Promise((resolve, reject) => {
                        openEditorBase(input, source).then((result) => {
                            if (result === null) {
                                let model = monaco.editor.getModel(input.resource);

                                if (model !== null) {
                                    source.setModel(model);
                                    target.activateModel(model.id, model.fileName, model.microService);
                                }
                            }

                            resolve(result);
                        });
                    });
                };

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
                if (v.dirty) {
                    var elapsed = Date.now() - v.timestamp;
                    var isTyping = elapsed < 500;

                    if (!isTyping) {
                        result.push(v);

                        v.timestamp = Date.now();
                        v.dirty = false;
                    }
                }
            });

            return result;
        },
        loadDecorations: function (e) {
            ide.designerAction({
                'data': {
                    'action': 'deltaDecorations',
                    'model': {
                        'uri': e.model.uri.toString(),
                        'id': e.model.id
                    },
                    'text': e.model.getValue(),
                },
                onComplete: (d) => {
                    this.setDecorations({
                        model: e.model,
                        decorations: d
                    });
                }
            });
        },
        setDecorations: function (e) {
            var existing = null;
            var decorations = this.options.decorations;

            if (decorations) {
                for (let i = 0; i < decorations.length; i++) {
                    let decoration = decorations[i];

                    if (decoration.id === e.model.id) {
                        existing = decoration;
                        break;
                    }
                }
            }

            var existingDecorations = existing && existing.decorations || [];
            var newDecorations = e.decorations || [];
            var result = e.model.deltaDecorations(existingDecorations, newDecorations);

            if (!existing) {
                decorations.push({
                    id: e.model.id,
                    decorations: result
                });
            }
            else
                existing.decorations = result;
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
                if (!existingState.dirty)
                    this.options.state.splice(existingStateIndex, 1);
                else {
                    existingState.dirty = true;
                    existingState.timestamp = Date.now();
                }
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

            if (features.definition)
                this._definition(language);

            if (features.signatureHelp)
                this._signatureHelp(language);

            if (features.documentSymbol)
                this._documentSymbol(language);

            if (features.documentFormatting)
                this._documentFormatting(language);

            if (features.codeLens)
                this._codeLens(language);

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
        _definition: function (language) {
            var instance = this;

            monaco.languages.registerDefinitionProvider(language, {
                provideDefinition: function (model, position) {
                    return new Promise(function (resolve, reject) {
                        try {
                            ide.designerAction({
                                data: {
                                    action: 'provideDefinition',
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
                                    if (!data || !data.uri) {
                                        resolve();
                                        return;
                                    }

                                    var uri = monaco.Uri.parse(data.uri);
                                    var model = monaco.editor.getModel(uri);

                                    if (model === null) {
                                        ide.designerAction({
                                            data: {
                                                action: 'loadModel',
                                                model: {
                                                    'uri': uri.toString()
                                                }
                                            },
                                            onComplete: (e) => {
                                                var model = monaco.editor.createModel(e.text, e.language, uri.toString());

                                                model.fileName = e.fileName;
                                                model.microService = e.microService;

                                                if (!textEditorState.isLanguageInitialized(e.language)) {
                                                    textEditorState.initializeLanguage(e.language, {
                                                        codeAction: e.codeAction,
                                                        completionItem: e.codeCompletion,
                                                        declaration: e.declaration,
                                                        definition: e.definition,
                                                        signatureHelp: e.signatureHelp,
                                                        documentSymbol: e.documentSymbol
                                                    });
                                                }


                                                resolve({
                                                    uri: uri,
                                                    range: data.range
                                                });
                                            }
                                        });
                                    }
                                    else {
                                        resolve({
                                            uri: uri,
                                            range: data.range
                                        });
                                    }
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
        },
        _documentFormatting: function (language) {
            var instance = this;

            monaco.languages.registerDocumentFormattingEditProvider(language, {
                provideDocumentFormattingEdits: function (model, options) {
                    return new Promise(function (resolve, reject) {
                        try {
                            ide.designerAction({
                                data: {
                                    action: 'provideDocumentFormattingEdits',
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
        },
        _codeLens: function (language) {
            var instance = this;

            monaco.languages.registerCodeLensProvider(language, {
                provideCodeLenses: function (model, cancel) {
                    return new Promise(function (resolve, reject) {
                        try {
                            ide.designerAction({
                                data: {
                                    action: 'provideCodeLens',
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
                                        resolve({
                                            lenses: data,
                                            dispose: () => {
                                            }
                                        });
                                    else
                                        resolve();
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
        activateModel: function (id, fileName, ms) {
            let models = monaco.editor.getModels();

            var tab = $(`div[data-model="${id}"]`);

            if (tab.length === 0) {
                var color = null;

                for (let i = 0; i < this.options.colors.length; i++) {
                    let cl = this.options.colors[i];

                    if (cl.ms === ms) {
                        color = cl.color;
                        break;
                    }
                }

                if (color === null) {
                    for (let i = 0; i < this.options.colors.length; i++) {
                        let cl = this.options.colors[i];

                        if (cl.ms === null) {
                            cl.ms = ms;
                            color = cl;
                            break;
                        }
                    }
                }

                if (color === null)
                    color = this.options.colors[0];

                $('#modelTabs').append(`<div class="col-auto"><div class="model-tab" data-model="${id}" style="background-color:${color.color}" title="${ms}">${fileName}</div></div>`);
            }

            $('div[data-model]').removeClass('active');

            for (let i = 0; i < models.length; i++) {
                let model = models[i];

                if (model.id === id) {
                    $(`div[data-model="${id}"]`).addClass('active');

                    this.options.instance.setModel(model);

                    break;
                }
            }
        },
        closeModel: function (id) {
            let models = monaco.editor.getModels();

            if (models.length < 2)
                return;

            for (let i = 0; i < models.length; i++) {
                let model = models[i];

                if (model.id === id) {
                    model.dispose();
                    $(`div[data-model="${id}"]`).remove();

                    if (i === 0)
                        this.activateModel(models[1].id);
                    else
                        this.activateModel(models[i - 1].id);

                    break;
                }
            }
        }
    });
})(jQuery);