(function (tompit, $, undefined) {
	'use strict';

	$.fn.exists = function () {
		return this.length !== 0;
	};

	$.fn.hasAttr = function (name) {
		var attr = this.attr(name);

		return typeof attr !== 'undefined' && attr !== false;
	};

	tompit.invariantDate = function (v) {
		if (typeof v === 'undefined' || v === null || !(v instanceof Date))
			return null;

		var r = new Date(v.getFullYear(), v.getMonth(), v.getDay());

		r.setUTCHours(v.getHours(), v.getMinutes(), v.getSeconds(), v.getMilliseconds());

		return r;
	};

	tompit.success = function (message, title, e) {
		toastr.success(message, title, $.extend({
            positionClass: 'tp-toast-bottom-center'
		}, e));
	};

	tompit.warning = function (message, title, e) {
		toastr.warning(message, title, $.extend({
			positionClass: 'tp-toast-bottom-center'
		}, e));
	};

	tompit.info = function (message, title, e) {
		toastr.info(message, title, $.extend({
            positionClass: 'tp-toast-bottom-center'
		}, e));
	};

	tompit.error = function (message, title, e) {
		toastr.error(message, title, $.extend({
			positionClass: 'tp-toast-bottom-center'
		}, e));
	};

	tompit.clientError = function (message, title) {
		tompit.error(message, title);

		throw message;
	};

    tompit.resolveErrorMessage = function (request) {
        try {
            var err = $.parseJSON(request.responseText);

            return err === null ? null : err.message;
        }
        catch (e) {
            return null;
        }
    };

	tompit.handleError = function (request, status, error) {
		var title = null;
		var message = null;
        var severity = 'critical';

		try {
			var err = $.parseJSON(request.responseText);

			title = err === null ? status : err.source;
			message = err === null ? null : err.message;
            severity = err === null ? 'critical' : typeof err.severity === 'undefined' ? critical : err.severity;

			var inner = err === null || typeof err.inner === 'undefined' ? null : err.inner;

			if (message === null)
				message = '';

			if (inner !== null)
				message += ' ' + inner;
		}
		catch (e) {
			message = error;
		}

		if (severity === 'info') {
			if (typeof title !== undefined && title !== null)
				tompit.info(message, title);
			else
				tompit.info(message, request.status);
		}
		else if (severity === 'warning') {
			if (typeof title !== undefined && title !== null)
				tompit.warning(message, title);
			else
				tompit.warning(message, request.status);
		}
		else {
			if (typeof title !== undefined && title !== null)
				tompit.error(message, title);
			else
				tompit.error(message, request.status);
		}

	};


	tompit.findProgress = function (s) {
        if (typeof s !== 'undefined') {
            var sel = s instanceof jQuery ? s : $(s);

            if (sel.attr('[data-tp-tag="progress-container"]') !== null)
                return sel.tpProgress('instance');

			var p = $('[data-tp-tag="progress-container"]', sel);

			if (p.length > 0)
				return p.tpProgress('instance');
		}

		return null;
	};

	tompit.resetContainer = function (options) {
		var progress = tompit.findProgress(options.container);

		if (progress !== null)
			progress.hide();

		if (typeof options.dependencies !== 'undefined') {
			$.each(options.dependencies, function (index, value) {
				$(value).prop('disabled', false);
			});
		}
	};
})(window.tompit = window.tompit || {}, jQuery);