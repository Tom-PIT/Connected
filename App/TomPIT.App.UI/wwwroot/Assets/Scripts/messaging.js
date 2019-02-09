;'use strict';

$.widget('tompit.tpMessaging', {
	options: {
		connections: []
	},
	create: function () {

	},
	connect: function (e) {
		var exists = false;
		var instance = this;

		$.each(this.options.connections, function (i, v) {
			if (v.url === e.url && v.microService === e.microService
				&& v.hub === e.hub && v.id === e.id) {
				exists = true;
				return false;
			}
		});

		if (exists)
			return;

		var qs = encodeURI('?microService=' + e.microService + '&hub=' + e.hub);

		var connection = {
			'url': e.url,
			'id': e.id,
			'microService': e.microService,
			'bindings': [],
			'appBaseUrl': e.appBaseUrl,
			'hub': e.hub,
			'connection': new signalR.HubConnectionBuilder()
				.withUrl(e.url + qs)
				.configureLogging(signalR.LogLevel.Information)
				.build()
		};

		connection.connection.on('data', function (data) {
			var views = [];

			$.each(connection.bindings, function (i, v) {
				var existingView = null;

				$.each(views, function (vi, vv) {
					if (vv.view === v.view) {
						existingView = vv;
						return false;
					}
				});

				if (existingView === null) {
					existingView = {
						view: v.view,
						stencils: []
					};

					views.push(existingView);
				}

				if (!existingView.stencils.includes(v.stencil))
					existingView.stencils.push(v.stencil);
			});

			$.each(views, function (i, v) {
				tompit.post({
					'url': connection.appBaseUrl + '/' + v.view,
					'data': v.stencils,
					onComplete: function (data) {
						//slice data and replace stencils
					}
				});

				instance.element.trigger('valueChanged', [data, connection.hub, v.view]);
			});
		});

		this._startConnection(connection.connection);

		connection.connection.onclose(function (){
			instance._startConnection(connection.connection);
		});

		this.options.connections.push(connection);
	},
	transaction: function (s, d, id) {
		if (typeof id === 'undefined')
			id = 'iot';

		var connection = this._getConnection(id);

		var hub = null;

		if (s !== null) 
			hub = $(s).closest('[data-hub]').attr('data-hub');

		d.hub = hub;
		d.microService = connection.microService;

		if (typeof d.hub === 'undefined')
			throw 'Hub not specified';

		connection.connection.invoke('transaction', d).catch(function (error) {
			tompit.error(error.toString());
		});
	},
	registerBindings: function (e, id) {
		if (typeof id === 'undefined')
			id = 'iot';

		var connection = this._getConnection(id);

		$.each(e, function (i, v) {
			var exist = false;

			$.each(connection.bindings, function (bi, bv) {
				if (v.stencil === bv.stencil && v.field === bv.field) {
					exist = true;
					return false;
				}
			});

			if (!exist)
				connection.bindings.push(v);
		});
	},
	_getConnection: function (id) {
		var r = null;

		$.each(this.options.connections, function (i, v) {
			if (v.id === id) {
				r = v;
				return false;
			}
		});

		return r;
	},
	_startConnection(c) {
		var instance = this;

		c.start().catch(function (err) {
			tompit.error(err.toString());

			setTimeout(function () {
				instance._startConnection(c);
			}, 2500);
		});
	}
});