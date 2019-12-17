'use strict';

$.widget('tompit.tpProgress', {
	options: {
		selector: null,
		disable: true,
		text: null,
        textCss: null,
        showAfter: 500
	},

	_create: function () {

	},
	show: function () {
		var active = this.element.attr('data-inprogress');

		if (typeof active !== 'undefined' && active !== false && active === '1')
			return;

		this.element.attr('data-inprogress', '1');

		if (this.options.disable === true)
			this.element.addClass('disabled');

		var instance = this;

		setTimeout(function () {
			var opt = instance.options;
			var e = instance.element;
			var att = e.attr('data-inprogress');

			instance._visible = true;

			if (typeof att !== 'undefined' && att !== false) {
				var txt = '';
				var progress = ' <div class="progress animated"><div class="indeterminate"></div></div>';
				var txtCss = opt.textCss === null ? '' : opt.textCss;

				if (opt.text !== null && opt.text.length > 0)
					txt = ' <div class="inprogresstext ' +  txtCss + '">' + opt.text + '</div>';

				var html = '<div data-kind="in-progress" class="clearfix"> ';

				e.html(html + txt + progress + '</div>');
			}
        }, options.showAfter);
	},
	hide: function () {
		this.element.removeAttr('data-inprogress');

		$('[data-kind="in-progress"]', this.element).remove();

		if (this.options.disable === true)
			this.element.removeClass('disabled');

		this._visible = false;

	},
	setText: function (value, css) {
		this.options.text = value;
		var oldCss = '';

		if (typeof css !== 'undefined' && this.options.css !== css) {
			oldCss = this.options.textCss;

			this.options.textCss = css;
		}

		var txt = $('.inprogresstext', this.element);

		if (txt.exists()) {
			txt.html(value);

			if (oldCss !== '' && oldCss !== null)
				txt.removeClass(oldCss);

			if (this.options.textCss !== null && this.options.textCss !== '')
				txt.addClass(this.options.textCss);
		}
		else {
			$('[data-kind="in-progress"]', this.element).prepend(' <div class="inprogresstext ' + this.options.textCss === null ? '' : this.options.textCss + '">' + value + '</div>');
		}
	},
	isVisible: function () {
		return this._visible;
	}
});

