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
	}
});