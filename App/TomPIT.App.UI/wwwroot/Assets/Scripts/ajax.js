(function (tompit, $, undefined) {
	'use strict';

	tompit.post = function (options) {
		var o = $.extend(true, tompit.ajaxDefaultOptions(), options);

		o.method = 'POST';

		ajaxRequest(o);
	};

	tompit.get = function (options) {
		var o = $.extend(true, tompit.ajaxDefaultOptions(), options);

		o.method = 'GET';

		ajaxRequest(o);
	};

	function ajaxRequest(options) {
        var data = options.onPrepareData(options.data),
            deps = {
                items: []
            };

        if (options.dependencies) {
            if ($.isPlainObject(options.dependencies)) {
                deps = options.dependencies;
            }
            else {
                deps.items = options.dependencies;
            }
        }

		$.ajax({
			url: options.url,
			type: options.method,
			cache: options.cache,
			data: JSON.stringify(data),
			contentType: options.contentType,
			beforeSend: function (request, settings) {
				options.onBeforeSend(request, settings, options.progress);

                if (typeof options.headers !== 'undefined') {
                    for (var key in options.headers) 
                        request.setRequestHeader(key, options.headers[key]);
                }

                if (deps.items.length > 0) {
                    $.each(deps.items, function (index, value) {
                        tompit.setElementDisabled($(value), true);
					});
				}

				if (options.progress)
					options.progress.show();
			},
			success: function (data, status, request) {
				options.onSuccess(data, status, request, options.progress);
			},
			complete: function (request, status) {
                options.onComplete(request, status, options.progress);

                if (!options.onQueryContinueProgress(request, status, options.progress)) {
                    if (deps.items.length > 0) {
                        $.each(deps.items, function (index, value) {
                            tompit.setElementDisabled($(value), false);
                        });
                    }

                    if (options.progress) {
                        options.progress.hide();
                    }
                }
                else {
                    if (options.progress) {
                        options.progress.show();
                    }
                }
			},
			error: function (request, status, error) {
				if (!options.onError(request, status, error, options.progress)) {
					tompit.handleError(request, status, error);
				}
			}
		});
	}

	tompit.ajaxDefaultOptions = function () {
		return {
			contentType: 'application/json; charset=UTF-8',
			dataType: 'json',
			url: null,
			method: 'POST',
			cache: false,
            data: null,
            headers: {},
			//callbacks
			onPrepareData: function (data) {
				return data;
			},
			onBeforeSend: function (request, settings, progress) {
				return false;
			},
			onSuccess: function (data, status, request, progress) {
				return false;
			},
			onComplete: function (request, status, progress) {
				return false;
			},
			onError: function (request, status, error, progress) {
				return false;
			},
			onQueryContinueProgress: function (progress) {
				return false;
			},
			// elements
			dependencies: [],
			progress: null
		}
	}
})(window.tompit = window.tompit || {}, jQuery);