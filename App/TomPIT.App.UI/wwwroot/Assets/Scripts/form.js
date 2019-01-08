'use strict';

$.widget('tompit.tpForm', {
	options: {
		text: {
			required: ''
		},
		appearance: {
			showValid:true,
		},
		onValidating: function (sender) {
			return true;
		}
	},
	isInputValid: function (s) {
		return !$(s).hasClass('is-invalid');
	},
	validate: function () {
		var form = this.element;

		this.resetValidation();

		var inputs = this._validateInputs();
		var areas = this._validateTextAreas();
		var tokens = this._validateTokens();
		var selects = this._validateSelects();

		if (!this.options.onValidating(this))
			return false;

		return inputs && tokens && areas && selects;
	},
	submit: function (options) {
		var defaultOptions = {
			progress: this.progress(),
			onHideProgress: options.onHideProgress,
			headers: {
				'RequestVerificationToken':this.verificationToken()
			},
			data: {
				
			}
		};

		$('.validation-summary-errors', this.form()).remove();

		tompit.post($.extend(true, defaultOptions, options));
	},
	form: function () {
		return this.element;
	},
	hasRedirectUrl: function (data) {
		return typeof data.responseJSON !== 'undefined'
			&& typeof data.responseJSON.url === 'undefined';
	},
	resetValidation: function () {
		var valids = $('.is-valid', this.form());
		var invalids = $('.is-invalid', this.form());

		$.each(valids, function (i, v) {
			$(v).removeClass('is-valid');
		});

		$.each(invalids, function (i, v) {
			$(v).removeClass('is-invalid');
		});
	},
	progress: function () {
		var selector = '[data-tp-tag="progress-container"]';

		return $(selector, this.element).tpProgress('instance');
	},
	getValue: function (selector) {
		var input = $(selector, $(this.element));

		if (input.parent().hasClass('tokenfield')) 
			return input.tokenfield('getTokensList');

		if (input.hasAttr('type')) {
			var attr = input.attr('type');

			if (attr === 'checkbox')
				return input[0].checked;
		}

		return input.val();
	},
	getButtons: function () {
		return $('button', this.form()).toArray();
	},
	verificationToken: function () {
		return $('input[name="__RequestVerificationToken"]', $(this.element)).val();
	},
	_getFieldName: function (input) {
		var r = input.attr('name');

		if (r !== null && r.length > 0)
			return r;

		return input.attr('id');
	},
	_validateSelects: function () {
		var selects = $('select', this.form());
		var instance = this;

		$.each(selects, function (index, value) {
			var s2 = $(value).data('select2');

			if (typeof s2 === 'undefined')
				return instance._validateRequired(value);
			else {
				var select = $(value).parent().children('select');
				var selection = $(value).select2('data');
				var val = selection == null || selection.length == 0 ? null : selection[0].id;

				if (!instance._validateRequired(select, val))
					$(value).closest('[data-component="select"]').addClass('is-invalid');
				else {
					if (instance.options.appearance.showValid)
						$(value).closest('[data-component="select"]').addClass('is-valid');
				}

			}
		});


		return true;
	},

	_validateTextAreas: function () {
		var inputs = $('textarea', this.element);
		var r = true;
		var instance = this;

		$.each(inputs, function (i, v) {
			if (!instance._validateRequired(v)
				|| !instance._validateMaxLength(v)
				|| !instance._validateInputType(v))
				r = false;
		});

		return r;
	},
	_validateInputs: function () {
		var inputs = $('input', this.element);
		var r = true;
		var instance = this;

		$.each(inputs, function (i, v) {
			if (!instance._validateRequired(v)
				|| !instance._validateMaxLength(v)
				|| !instance._validateInputType(v))
				r = false;
		});

		return r;
	},
	_validateTokens: function () {
		var tokens = $('.tokenfield', this.form());
		var r = true;
		var instance = this;

		$.each(tokens, function (i, v) {
			var input = $('input', $(v));

			if (!input.exists())
				return true;

			input = $(input.get(0));
			var value = input.tokenfield('getTokensList');

			if (!instance._validateRequired(input, value))
				r = false;

			if (input.hasClass('is-invalid'))
				$(v).addClass('is-invalid');
			else if(input.hasClass('is-valid'))
				$(v).addClass('is-valid');
		});

		return r;
	},
	_validateRequired: function (input, value) {
		var e = $(input);

		if (!e.hasAttr('required'))
			return true;

		var val = typeof value === 'undefined' ? e.val() : value;

		if (val === null || val.trim().length === 0) {
			this._setInvalid(e, 'required');

			return false;
		}

		this._setValid(e);

		return true;
	},
	_validateMaxLength: function (input, value) {
		var e = $(input);

		if (!e.hasAttr('maxlength'))
			return true;

		var maxLength = e.attr('maxlength');
		var val = typeof value === 'undefined' ? e.val() : value;

		if (val === null)
			return true;

		if (val.length > maxLength) {
			this._setInvalid(e, 'maxlength');

			return false;
		}

		this._setValid(e);

		return true;
	},
	_validateInputType: function (input) {
		var type = $(input).attr('type');

		if (type === 'number')
			this._validateNumber(input);

		return true;
	},

	_validateNumber: function (input) {
		return this._validateMin(input)
			|| this._validateMax(input);
	},

	_validateMin: function (input) {
		var e = $(input);

		if (!e.hasAttr('min'))
			return true;

		var min = e.attr('min');
		var val = e.val();

		if (val === null)
			return true;

		try {
			var num = parseFloat(val);

			if (num < min)
				this._setInvalid(e, 'min');

			return true;
		}
		catch (e) {
			this._setInvalid(e, 'min');

			return false;
		}
	},

	_validateMax: function (input) {
		var e = $(input);

		if (!e.hasAttr('max'))
			return true;

		var max = e.attr('max');
		var val = e.val();

		if (val === null)
			return true;

		try {
			var num = parseFloat(val);

			if (num > max)
				this._setInvalid(e, 'max');

			return true;
		}
		catch (e) {
			this._setInvalid(e, 'max');

			return false;
		}
	},

	_setValid: function (e) {
		if (e.hasClass('is-invalid'))
			return;

		if (this.options.appearance.showValid)
			e.addClass('is-valid');
	},
	_setInvalid: function (e, kind) {
		if (e.hasClass('is-valid'))
			e.removeClass('is-valid');

		e.addClass('is-invalid');

		if (typeof kind !== 'undefined') {
			var att = 'data-val-' + kind;

			if (e.hasAttr(att)) {
				var target = e.siblings('.invalid-feedback');

				if (target.exists())
					target.html(e.attr(att));
			}
		}
	}
});

$.widget('tompit.tpSelect', {
	options: {
		size: '',
		search: {
			enabled:true
		},
		onChange: function (data) {
			return true;
		}
	},

	_create: function () {
		var s = {
			size: this.options.size
		};

		if (!this.options.search.enabled) 
			s.minimumResultsForSearch = Infinity;

		this.element.select2(s);

		var instance = this;

		this.element.change(function () {
			var selection = instance.getSelection();
			var value = selection == null || selection.length == 0 ? null : selection[0].id;

			instance.options.onChange({
				selection: selection,
				value: value
			});
		});
	},

	getValue: function () {
		var selection = this.getSelection();

		if (selection == null || selection.length == 0) {
			return {
				text: null,
				id: null

			}
		}

		return selection[0];
	},

	getSelection: function () {
		return $(this.element).select2('data');
	},
	setValue: function(value) {
		this.element.val(value);
	},
	clear: function (value) {
		var select = $('option', this.element);

		select.remove();
		this.setValue(null);
	},
	fill: function (data) {
		if (typeof data === 'undefined' || data == null)
			return;

		var instance = this.element;
		
		$.each(data, function (i, v) {
			var item = new Option(v.text, v.value, false, false);

			instance.append(item);
		});

		if (data.length > 0) {
			var first = data[0];

			instance.val(first.value);
			instance.trigger('change');
		}
	}

});