﻿'use strict';

$.widget('tompit.tpIoT', {
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
				.build(),
			'getBindings': function (field) {
				var r = [];

				$.each(this.bindings, function (i, v) {
					if (v.field === field)
						r.push(v);
				});

				return r;
			}
		};

		connection.connection.on('data', function (data) {
			var views = [];

			$.each(data, function (i, v) {
				if (i === '$timestamp')
					return true;

				var bindings = connection.getBindings(i);

				$.each(bindings, function (i, v) {
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
			});

			$.each(views, function (i, v) {
				instance.reloadStencils({
					'view': v.view,
					'connection': connection.id,
					'stencils': v.stencils,
					'data': data
				});

				instance.element.trigger('valueChanged', [data, connection.hub, v.view]);
			});

			instance.element.trigger('valueChanged', [data, connection.hub, null]);

		});

		this._startConnection(connection.connection);

		connection.connection.onclose(function (){
			instance._startConnection(connection.connection);
		});

		this.options.connections.push(connection);
	},
	transaction: function (d) {
		if (typeof d.connection === 'undefined')
			d.connection = 'iot';

		var connection = this._getConnection(d.connection);

		if (typeof d.hub === 'undefined') {
			if (typeof d.sender !== 'undefined')
				d.hub = $(d.sender).closest('[data-hub]').attr('data-hub');
		}

		d.microService = connection.microService;

		if (typeof d.hub === 'undefined')
			throw 'Hub not specified';

		delete d.sender;
		delete d.connection;

		connection.connection.invoke('transaction', d).catch(function (error) {
			tompit.error(error.toString());
		});
	},
	reloadStencils: function (e) {
		var connection = this._getConnection(e.connection);
		var view = e.view;

		tompit.post({
			'url': encodeURI(connection.appBaseUrl + '/' + connection.microService + '/' + view),
			'data': {
				'data': e.data,
				'stencils': e.stencils,
				'fields': e.fields
			},
			onComplete: function (data) {
				var result = $('<div></div>').append(data.responseText).children('[data-stencil]');

				$.each(result, function (i, v) {
					var html = $(v).html();
					var id = $(v).attr('data-stencil');
					var targetHtml = $(html);
					var svg = d3.select('[data-iot="' + view + '"] svg');
					var stencil = svg.select('#' + id);
					var attributes = targetHtml[0].attributes;

					if (targetHtml.length > 0) {
						$.each(attributes, function (i, v) {
							stencil.attr(v.name, v.value);
						});
					}

					$.each(stencil.node().attributes, function (i, v) {
						var exists = false;

						$.each(attributes, function (ii, vv) {
							if (vv.name === v.name) {
								exists = true;
								return false;
							}
						});

						if (!exists)
							stencil.node.removeAttr(v.name);
					});

					stencil.html(targetHtml.html());
				});
			}
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
	getValue: function (e) {
		tompit.post({
			'url': 'iot',
			'data': e
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