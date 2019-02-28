(function (tompit, $, undefined) {
	'use strict';

	tompit.DEFAULTS = {
		appUrl: null,
		restUrl: null,
		component: null
	};

	tompit.GLOBALIZE = {
		appUrlNotSet: 'Application URL not set',
		restUrlNotSet: 'REST URL not set'
	};

	tompit.invoke = function (e) {
		var progress = tompit.findProgress(e.container);

		var options = $.extend(tompit.ajaxDefaultOptions(), e, {
			data: e.parameters,
			progress: progress
		});

		var api = options.api;

		delete options.api;
		delete options.parameters;

		if (options.data === null)
			options.data = {};

		options.data.__api = api;
		options.data.__component = tompit.DEFAULTS.component;
		options.url = tompit.url().api('invoke');

		tompit.post(options);
	};

	tompit.partial = function (e) {
		var progress = tompit.findProgress(e.container);

		var options = $.extend(tompit.ajaxDefaultOptions(), e, {
			data: e.parameters,
			progress: progress
		});

		var name = options.name;

		delete options.name;
		delete options.parameters;

		if (options.data === null)
			options.data = {};

		options.data.__name = name;
		options.data.__component = tompit.DEFAULTS.component;
		options.url = tompit.url().api('partial');

		tompit.post(options);
	};

	tompit.setUserData = function (e) {
		var options = $.extend(tompit.ajaxDefaultOptions(), e, {
			data: {
				'data': e
			}
		});

		options.url = tompit.url().api('setUserData');

		tompit.post(options);
	};

	tompit.getUserData = function (e) {
		var options = $.extend(tompit.ajaxDefaultOptions(), e, {
			data: {
				'data':e
			}
		});

		options.url = tompit.url().api('getUserData');
		tompit.post(options);
	};

	tompit.queryUserData = function (e) {
		var options = $.extend(tompit.ajaxDefaultOptions(), e, {
			data: {
				'data': e
			}
		});

		options.url = tompit.url().api('queryUserData');

		tompit.post(options);
	};

	tompit.url = function () {
		this.api = function (verb) {

			if (tompit.DEFAULTS.appUrl === null)
				tompit.clientError(tompit.GLOBALIZE.appUrlNotSet);

			return tompit.DEFAULTS.appUrl + '/sys/api/' + verb;
		};

		return this;
	};
})(window.tompit = window.tompit || {}, jQuery);