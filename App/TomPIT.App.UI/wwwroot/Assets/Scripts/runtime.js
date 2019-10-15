(function (tompit, $, undefined) {
	'use strict';

	tompit.DEFAULTS = {
		appUrl: null,
		restUrl: null,
		component: null
	};

	tompit.GLOBALIZE = {
		appUrlNotSet: 'Application URL not set'
	};

    tompit.isView = function (request) {
        return request.getResponseHeader('X-TP-VIEW') !== null;
    };

    tompit.apiUrl = function(e) {
        return tompit.url().api('invoke');
    };

	tompit.invoke = function (e) {
		var progress = tompit.findProgress(e.container);
        
		var options = $.extend(tompit.ajaxDefaultOptions(), e, {
			data: e.parameters,
			progress: progress
		});

        if (typeof options.headers['X-TP-AF'] === 'undefined') {
            var antiforgery = tompit.antiForgeryValue();

            if (antiforgery !== null)
                options.headers['X-TP-AF'] = antiforgery;
        }

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

    tompit.search = function (e) {
        var progress = tompit.findProgress(e.container);

        var options = $.extend(tompit.ajaxDefaultOptions(), e, {
            data: e.parameters,
            progress: progress
        });

        if (options.data === null)
            options.data = {};

        options.url = tompit.url().api('search');

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

    tompit.parseUrl = function (template, parameters) {
        var tokens = template.split('/');
        var compiled = '/';

        $.each(tokens, function (i, v) {
            var token = v;

            if (token.startsWith('{') && token.endsWith('}') && typeof parameters !== 'undefined') {
                var key = token.substr(1, token.length - 2);
                var match = false;

                $.each(parameters, function (ii, vv) {
                    if (vv.key === key) {
                        compiled += vv.value + '/';
                        match = true;

                        return false;
                    }
                });

                if (!match)
                    compiled += token + '/';
            }
            else
                compiled += token + '/';
        });

        return compiled.length > 0 ? compiled.substr(0, compiled.length - 1) : compiled;
    };

    tompit.isValidDate = function (value) {
        //0001-01-01T00:00:00+01:00

        if (typeof value === 'undefined' || value.length === 0)
            return false;

        var tokens = value.split('-');

        if (tokens.length < 3)
            return false;

        if (tokens[0] === '0001' && tokens[1] === '01' && tokens[2].startsWith('01'))
            return false;

        return true;
    };

    tompit.antiForgeryValue = function () {
        var target = $('input[name="TomPITAntiForgery"]');

        if (target.length === 0)
            return null;

        return target.val();
    };

    tompit.createIotHub = function (selector) {
        return $(selector).tpIoT().data('tompit-tpIoT');
    };
})(window.tompit = window.tompit || {}, jQuery);