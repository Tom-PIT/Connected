'use strict';

$.widget('tompit.tpDeployment', {
	options: {

	},
	_create: function () {
		var instance = this;

		$('[data-button="install"]').click(function () {
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

		$('#aLogoff').click(function () {
			ide.designerAction({
				data: {
					'action': 'logoff'
				},
				onComplete: function (data) {
					$('#devDesigner').html(data);
				}
			});
		});

		$('#aLogin').click(function () {
			ide.designerAction({
				data: {
					'action': 'login'
				},
				onComplete: function (data) {
					$('#devDesigner').html(data);
				}
			});
		});

		$('#btnShowList').click(function () {
			$('#mainPackage').collapse('hide');
			$('#packageList').collapse('show');
			$('#packagePeek').collapse('hide');
		});

		$('#btnInstallPackages').click(function () {
			ide.designerAction({
				'data': {
					'action': 'installConfirm',
					'packages': [instance.options.mainPackage]
				},
				onComplete: function (data) {
					$('#mainPackage').collapse('hide');
					$('#packageList').collapse('show');
					$('#packagePeek').collapse('hide');
				}
			});
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
	},
	resolvePackageId: function (e) {
		return e.closest('[data-id]').attr('data-id');
	}
});