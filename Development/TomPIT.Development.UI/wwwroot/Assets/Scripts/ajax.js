(function (tompit, $, undefined) {
	'use strict';

	tompit.post=function(options) {
		var o = $.extend(true, ajaxDefaultOptions(), options);

		o.method = 'POST';

		return ajaxRequest(o);
	};

	tompit.get=function(options) {
		var o = $.extend(true, ajaxDefaultOptions(), options);

		o.method = 'GET';

		return ajaxRequest(o);
	};

	function ajaxRequest(options) {
		var data = options.onPrepareData(options.data);

		return $.ajax({
			url: options.url,
			type: options.method,
			cache: options.cache,
			data: JSON.stringify(data),
			contentType: options.contentType,
			headers:options.headers,
			beforeSend: function (request, settings) {
				options.onBeforeSend(request, settings, options.progress);

				if (options.dependencies.length > 0) {
					$.each(options.dependencies, function (index, value) {
						$(value).prop('disabled', true);
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
					if (options.dependencies) {
						$.each(options.dependencies, function (index, value) {
							$(value).prop('disabled', false);
						})
					}

					if (options.progress)
						options.progress.hide();
				} else {
					if (options.progress)
						options.progress.show();
				}
			},
			error: function (request, status, error) {
				if (!options.onError(request, status, error, options.progress)) {
					tompit.handleError(request, status, error);
				}
			}
		});
	}

	function ajaxDefaultOptions() {
		return {
			contentType: 'application/json; charset=UTF-8',
			dataType:'json',
			url: null,
			method: 'POST',
			cache: false,
			data: null,
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