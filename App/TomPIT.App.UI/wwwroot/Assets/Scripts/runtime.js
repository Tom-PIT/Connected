(function (tompit, $, undefined) {
    'use strict';

    tompit.DEFAULTS = {
        appUrl: null,
        restUrl: null,
        component: null,
        view: null
    };

    tompit.invokeInjectors = [];

    tompit.GLOBALIZE = {
        appUrlNotSet: 'Application URL not set'
    };

    tompit.isView = function (request) {
        return request.getResponseHeader('X-TP-VIEW') !== null;
    };

    tompit.uiInjection = function (e) {
        e = e || {};
        var progress = tompit.findProgress(e.container);

        var options = $.extend(tompit.ajaxDefaultOptions(), e, {
            data: e.parameters,
            progress: progress
        });

        options.data = options.data || {};
        options.url = tompit.url().api('uiInjection');

        options.data.__component = tompit.DEFAULTS.component;

        if (typeof options.view === 'undefined')
            options.data.__view = tompit.DEFAULTS.view;
        else
            options.data.__view = options.view;

        if (typeof options.__viewUrl === 'undefined')
            options.data.__viewUrl = window.location.href;

        if (typeof options.headers['X-TP-AF'] === 'undefined') {
            var antiforgery = tompit.antiForgeryValue();

            if (antiforgery !== null)
                options.headers['X-TP-AF'] = antiforgery;
        }

        delete options.view;
        delete options.parameters;

        options.onSuccess = function (data) {
            var content = $(data);
            var items = $('ul > li', content);

            $.each(items, function (i, v) {
                var partial = $(v);
                var selector = partial.attr('data-selector');
                var mode = partial.attr('data-inject');

                if (!selector) {
                    selector = document.body;
                    mode = 'Append';
                }

                if (mode === 'Before')
                    $(selector).before(partial.html());
                else if (mode === 'Prepend')
                    $(selector).prepend(partial.html());
                else if (mode === 'After')
                    $(selector).after(partial.html());
                else if (mode === 'Replace')
                    $(selector).html(partial.html());
                else
                    $(selector).append(partial.html());
            });
        };

        tompit.post(options);
    };

    tompit.apiUrl = function (e) {
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

        var promises = [];

        $.each(tompit.invokeInjectors, function (i, v) {
            if (v.api && v.api !== api)
                return;

            if (v.provideData) {
                var promise = v.provideData(options.data);
                promises.push(promise);

                promise.then((data) => {
                    if (data)
                        $.extend(options.data, data);
                });
            }
        });

        Promise.all(promises).then(() => {
            promises = [];
            var cancel = false;

            $.each(tompit.invokeInjectors, function (i, v) {
                if (v.api && v.api !== api)
                    return;

                if (v.invoking) {
                    var promise = v.invoking(options.data);
                    promises.push(promise);

                    promise.then((result) => {
                        if (!result)
                            cancel = true;
                    });
                }

                if (cancel)
                    return false;
            });

            Promise.all(promises).then(() => {
                promises = [];

                if (cancel)
                    return;

                options.onSuccessCompleting = function (data, status, request) {
                    return new Promise((resolve, reject) => {
                        var result = data;

                        try {
                            $.each(tompit.invokeInjectors, function (i, v) {
                                if (v.api && v.api !== api)
                                    return;

                                if (v.extendData) {
                                    var promise = v.extendData(options.data, data);
                                    promises.push(promise);

                                    promise.then((r) => {
                                        result = r;
                                    });
                                }
                            });

                            Promise.all(promises).then(() => {
                                resolve(result);
                            });
                        }
                        catch (ex) {
                            reject(ex);
                        }
                    });
                };

                tompit.post(options);
            });
        });
    };

    tompit.registerInvokeInjector = function (e) {
        var exists = false;

        $.each(tompit.invokeInjectors, function (i, v) {
            if (typeof e.name === 'undefined')
                return false;

            if (v.name === e.name) {
                v = e;
                exists = true;
                return false;
            }
        });

        if (!exists)
            tompit.invokeInjectors.push(e);
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

        if (typeof options.headers['X-TP-AF'] === 'undefined') {
            var antiforgery = tompit.antiForgeryValue();

            if (antiforgery !== null)
                options.headers['X-TP-AF'] = antiforgery;
        }

        options.onSuccessCompleted = function () {
            var injectionOptions = {
                data: {
                    'partial': name,
                    '__viewUrl': window.location.href
                }
            };

            $.extend(injectionOptions.data, options.data);

            tompit.uiInjection(injectionOptions);
        };

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
                'data': e
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


    tompit.setElementDisabled = function (element, disabled) {
        var $elt = $(element);
        $elt.prop('disabled', disabled);
        $elt.data('tp-disabled', disabled);
    };

    tompit.getElementDisabled = function (element) {
        var $elt = $(element);
        if ($elt.prop('disabled')) {
            return true;
        }
        if ($elt.data('tp-disabled')) {
            return true;
        }
        return false;
    };
})(window.tompit = window.tompit || {}, jQuery);