(function (tompit, $, undefined) {
	'use strict';


	tompit.initContainer = function (container) {
		$('[data-tp-tag="progress-container"]', container).tpProgress();
	}

	tompit.initShell = function () {
		window.addEventListener('resize', syncSidebar);

		function syncSidebar() {
			if ($(window).width() > 1199) {
				$('button[data-target="#_sideBar"]').removeClass('collapsed');
				$('#_sideBar').addClass('show');
			} else {
				$('button[data-target="#_sideBar"]').addClass('collapsed');
				$('#_sideBar').removeClass('show');
			}
		}

		syncSidebar();
	}

	tompit.initShell();
	tompit.initContainer();
})(window.tompit = window.tompit || {}, jQuery);