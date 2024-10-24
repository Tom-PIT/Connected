﻿'use strict';

$.widget('tompit.tpDeployment', {
	options: {
		checkTimestamp:0
	},
	_create: function () {
		var instance = this;

		this.initCards();

		$('#btnShowList').click(function () {
			$('#mainPackage').collapse('hide');
			$('#packageList').collapse('show');
			$('#packagePeek').collapse('hide');
		});

		$('#btnInstallPackages').click(function () {
			ide.designerAction({
				'data': {
					'action': 'installConfirm',
					'package': instance.options.mainPackage
				},
				onComplete: function (data) {
					instance.checkStates();

					$('#mainPackage').collapse('hide');
					$('#packageList').collapse('show');
					$('#packagePeek').collapse('hide');
				}
			});
		});

		this.options.timer = window.setInterval(function () {
			instance.checkStates();
		}, 2500);
	},
	_destroy: function () {
		window.clearInterval(this.options.timer);
	},
	initCards: function (s) {
		var instance = this;
		var items = typeof s === 'undefined'
			? $('[data-button="install"]')
			: $('[data-button="install"]', s);

		items.click(function () {
			var pck = $(this).closest('[data-id]').attr('data-id');

			ide.designerAction({
				'data': {
					'action': 'install',
					'package': pck
				},
				onComplete: function (data) {
					$('#packageContent').html(data);
					$('#mainPackage').collapse('show');
					$('#packageList').collapse('hide');
					$('#packagePeek').collapse('hide');

					instance.initPackage();
				}
			});
		});
	},
	checkStates: function () {
		var instance = this;

		ide.designerAction({
			'data': {
				'action': 'getChanges',
				'date': this.options.checkTimestamp
			},
			onComplete: function (data) {
				instance.options.checkTimestamp = data.timestamp;

				$.each(data.packages, function (i, v) {
					var s = $('.package-card[data-id="' + v + '"]');

					if (s.length === 0)
						return true;

					ide.designerAction({
						'data': {
							'action': 'getCard',
							'package': v
						},
						onComplete: function (data) {
							s.fadeOut('slow', function () {
								$(this).replaceWith(data);
								s = $('.package-card[data-id="' + v + '"]');

								s.fadeIn('slow');
								instance.initCards(s);
							});

						}
					});
				});
			}
		});
	},
	setMainPackage: function (v) {
		this.options.mainPackage = v;
	},
	initPackage: function () {
		var instance = this;

		$('[data-kind="runtime-configuration"]').click(function () {
			var value = $(this).attr('data-value');
			
			ide.designerAction({
				'data': {
					'action': 'setOption',
					'package': instance.resolvePackageId($(this)),
					'option': 'runtimeConfiguration',
					'value':value
				}
			});

			var glyph = 'fal fa-check-square';

			if (value === 'True')
				value = 'False';
			else {
				value = 'True';
				glyph = 'fal fa-times-square';
			}
			
			$(this).attr('data-value', value);

			$(this).html('<i class="fal ' + glyph + '"></i>');
		});

		$('[data-kind="resource-group"]').click(function () {
			var value = $(this).attr('data-value');

			ide.designerAction({
				'data': {
					'action': 'setOption',
					'package': instance.resolvePackageId($(this)),
					'option': 'resourceGroup',
					'value': value
				}
			});

			var text = $(this).html();

			$(this).closest('.dropdown').children('a').html(text);
		});
		
		$('[data-kind="connection-string"]').click(function () {
			var activated = $(this).attr('data-activated');
			var div = $(this);

			if (activated === 'true')
				return;
			
			$(this).attr('data-activated', 'true');

			$(this).find('[data-kind="text"]').collapse('hide');
			$(this).find('[data-kind="editor"]').dxTextArea({
				'value': $(this).attr('data-value'),
				onFocusOut: function (e) {
					var value = e.component.option('value');

					e.component.dispose();

					div.attr('data-activated', 'false');
					div.attr('data-value', value);

					ide.designerAction({
						'data': {
							'action': 'setOption',
							'package': instance.resolvePackageId(div),
							'option': 'connectionString',
							'value': value,
							'database': div.closest('[data-db]').attr('data-db')
						}
					});

					if (value === null || value.length === 0)
						div.find('[data-kind="text"]').html('(not set)');
					else
						div.find('[data-kind="text"]').html(value);

					div.find('[data-kind="text"]').collapse('show');
				}
			});

			$(this).find('[data-kind="editor"]').dxTextArea('instance').focus();
		});

		$('[data-kind="test-connection"]').click(function () {
			ide.designerAction({
				'data': {
					'action': 'testConnection',
					'package': instance.resolvePackageId($(this)),
					'value': instance.resolveConnectionString($(this)),
					'database': instance.resolveDatabase($(this))
				}
			});
		});

		$('[data-kind="create-database"]').click(function () {
			ide.designerAction({
				'data': {
					'action': 'createDatabase',
					'package': instance.resolvePackageId($(this)),
					'value': instance.resolveConnectionString($(this)),
					'database': instance.resolveDatabase($(this))
				}
			});
		});

		$('[data-kind="database-enabled"]').click(function () {
			var value = $(this).attr('data-value');

			ide.designerAction({
				'data': {
					'action': 'setOption',
					'package': instance.resolvePackageId($(this)),
					'database': instance.resolveDatabase($(this)),
					'option': 'enabled',
					'value': value
				}
			});

			var glyph = 'fal fa-check-square';

			if (value === 'True')
			{
				value = 'False';
				$(this).closest('[data-db]').removeClass('not-enabled');
			}
			else {
				value = 'True';
				glyph = 'fal fa-times-square';

				$(this).closest('[data-db]').addClass('not-enabled');
			}

			$(this).attr('data-value', value);
			$(this).children('svg').replaceWith('<i class="fal ' + glyph + '"></i>');
		});

		$('[data-kind="dependency-enabled"]').click(function () {
			var value = $(this).attr('data-value');
			var dependency = $(this).attr('data-dependency');

			ide.designerAction({
				'data': {
					'action': 'setOption',
					'package': instance.options.mainPackage,
					'dependency': dependency,
					'option': 'setDependency',
					'value': value
				}
			});

			var glyph = 'fal fa-check-square';

			if (value === 'True') {
				$(this).closest('[data-kind="dependency"]').children('.dependency-content').collapse('show');
				value = 'False';
			}
			else {
				value = 'True';
				glyph = 'fal fa-times-square';
				$(this).closest('[data-kind="dependency"]').children('.dependency-content').collapse('hide');
			}

			$(this).attr('data-value', value);
			$(this).children('svg').replaceWith('<i class="fal ' + glyph + '"></i>');
		});
	},
	resolvePackageId: function (e) {
		return e.closest('[data-id]').attr('data-id');
	},
	resolveDatabase: function (e) {
		return e.closest('[data-db]').attr('data-db');
	},
	resolveConnectionString: function (e) {
		return e.closest('[data-db]').find('[data-kind="connection-string"]').attr('data-value');
	}
});