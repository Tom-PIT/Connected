(function ($) {
	'use strict';

	$.widget('tompit.tpTextEditor', {

		options: {
			onCreated: function (instance) {
				return false;
			},
			onChanged: function (value) {
				return false;
			},
			instance:null
		},

		_create: function () {
			this._editors = [];
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

				if (target.options.instance !== null) {
					try {
						if (typeof target.options.instance.completionItemProvider !== 'undefined')
							target.options.instance.completionItemProvider.dispose();

						if (typeof target.options.instance.signatureHelpProvider !== 'undefined')
							target.options.instance.signatureHelpProvider.dispose();

						if (typeof target.options.instance.hoverProvider !== 'undefined')
							target.options.instance.hoverProvider.dispose();

						if (typeof target.options.instance.codeLensProvider !== 'undefined')
							target.options.instance.codeLensProvider.dispose();

						if (typeof target.options.instance.definitionProvider!== 'undefined')
							target.options.instance.definitionProvider.dispose();

						target.options.instance.dispose();
						target.options.instance = null;
					}
					catch (e) {
						console.log(e);
					}
				}

				options = $.extend({
					readOnly:false
				}, options);

				var src = options.source === null || options.source.length === 0 ? '\n' : options.source.join('\n');

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
					parameterHints: true,
					wrappingColumn: 0,
					wrappingIndent: 'indent',
					readOnly: options.readOnly
				});

				target.options.instance.addAction({
					id: '5CC30D480C3E4742B30A4FCEC5C4C7D8',
					label: 'Full Screen',
					keybindings: [monaco.KeyMod.CtrlCmd | monaco.KeyCode.F11],
					contextMenuGroupId: 'navigation',
					contextMenuOrder: 2.5,
					run: function (ed) {
						var container = $(ed.domElement).closest('[data-fullscreen="true"]');

						if (container.length > 0) {
							container.toggleClass("full-screen");
							container.toggleClass("code-editor-sa");
						}
					}
				});

				if ($.isFunction(options.onCreated))
					options.onCreated(target.options.instance);

				$('textarea', target.element).on('blur', function () {
					if(typeof options.onChange !== 'undefined')
						options.onChange(target.options.instance.getValue());
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
		}

	});
})(jQuery);