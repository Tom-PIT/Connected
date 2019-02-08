'user strict';

$.widget('tompit.iotDesigner', {
	options: {
		
	},
	_create: function () {
		var instance = this;

		this.element.droppable({
			greedy: true,
			accept: function (e) {
				return e.hasClass('dev-toolbox-item');
			},
			drop: function (e, ui) {
				ide.designerAction({
					'data': {
						'action': 'add',
						'id': ui.draggable.attr('data-id'),
						'x': ui.offset.left - $(this).offset().left,
						'y': ui.offset.top - $(this).offset().top
					},
					onComplete: function (data) {
						var id = $(data).attr('data-id');

						$('#iotDesigner').append(data);
						$('#iotDesigner').focus();
						instance.resizeCanvas();
						instance.initStencils();
						instance.unselectAll({
							onComplete: function () {
								instance.select({
									'id': id
								});
							}
						});
					}
				});
			}
		});

		this.element.selectable({
			'filter': '[data-select="true"]',
			'tolerance':'touch',
			selected: function (e, ui) {
				instance.selectionChanged();
			},
			unselected: function (e, ui) {
				instance.selectionChanged();
			}
		});

		this.element.click(function (e) {
			var args = {
				'element': $(e.target)
			};

			$('#iotDesigner').focus();

			if (e.ctrlKey || e.shiftKey) 
				instance.toggleSelection(args);
			else {
				var selected = instance.isSelected(args);

				instance.unselectAll({
					onComplete: function () {
						if (!selected)
							instance.select(args);
					}
				});
			}
		});

		this.initStencils();
		this.setupToolbar();
		this.syncToolbar();

		$('#iotDesigner').keyup(function (e) {
			if (e.keyCode === 46)
				instance.toolbar().delete.trigger();
		});
	},
	initStencils: function (s) {
		var instance = this;
		var offset = { top: 0, left: 0 };

		$.each($('[data-resize="true"]'), function (i, v) {
			var t = $(v);

			if (!t.data('resizable')) {
				t.resizable({
					handles: 'all',
					ghost: true,
					start: function (e) {
						var args = {
							'element': $(this)
						};

						if (!instance.isSelected(args)) {
							instance.unselectAll();
							instance.select(args);
						}
					},
					resize: function (e, ui) {
						var left = ui.position.left - ui.originalPosition.left;
						var top = ui.position.top - ui.originalPosition.top;
						var width = ui.size.width - ui.originalSize.width;
						var height = ui.size.height - ui.originalSize.height;

						var selected = instance.selectedStencils();
						var id = instance.getId({
							'element': $(this)
						});

						$.each(selected, function (i, v) {
							var t = $(v);
							var targetId = instance.getId({
								'element': t
							});

							if (targetId !== id) {
								var targetLeft = t.position().left + left;
								var targetTop = t.position().top + top;
								var targetWidth = t.width() + width;
								var targetHeight = t.height() + height;

								if (t.data('resizable'))
									t.resizable('disable');

								t.css({
									top: targetTop + 'px',
									left: targetLeft + 'px',
									width: targetWidth + 'px',
									height: targetHeight + 'px'
								});

								if (t.data('resizable'))
									t.resizable('enable');
							}
						});
					},
					stop: function (e, ui) {
						var args = {
							'element': $(this)
						};

						var elements = [];
						var selection = instance.selectedStencils();

						$.each(selection, function (i, v) {
							var tArgs = {
								'element': $(v)
							};

							elements.push({
								'id': instance.getId(tArgs),
								'Left': tArgs.element.position().left,
								'Top': tArgs.element.position().top,
								'Width': tArgs.element.width(),
								'Height': tArgs.element.height()
							});
						});

						ide.designerAction({
							'data': {
								'action': 'saveProperties',
								'elements': elements
							},
							onComplete: function (e) {
								instance.resizeCanvas();
							}
						});
					}

				});
			}
		});

		$.each($('[data-move="true"]'), function (i, v) {
			var t = $(v);

			if (!t.data('draggable')) {
				t.draggable({
					start: function (e, ui) {
						var args = {
							'element': $(this)
						};

						if (!instance.isSelected(args)) {
							instance.unselectAll();
							instance.select(args);
						}

						var selection = instance.selectedStencils();

						$.each(selection, function (i, v) {
							$(v).data("offset", $(v).offset());
						});

						offset = $(this).offset();
					},
					drag: function (e, ui) {
						var top = ui.position.top - offset.top;
						var left = ui.position.left - offset.left;
						var selected = instance.selectedStencils();
						var id = instance.getId({
							'element': $(this)
						});

						$.each(selected, function (i, v) {
							var t = $(v);
							var targetId = instance.getId({
								'element': t
							});

							if (targetId !== id) {
								var offset = t.data("offset");

								t.css({
									top: offset.top + top,
									left: offset.left + left
								});
							}
						});
					},
					stop: function (e, ui) {
						var args = {
							'element': $(this)
						};

						var elements = [];
						var selection = instance.selectedStencils();

						$.each(selection, function (i, v) {
							var tArgs = {
								'element': $(v)
							};

							elements.push({
								'id': instance.getId(tArgs),
								'Left': tArgs.element.position().left,
								'Top': tArgs.element.position().top
							});
						});

						ide.designerAction({
							'data': {
								'action': 'saveProperties',
								'elements': elements
							},
							onComplete: function (e) {
								instance.resizeCanvas();
							}
						});
					}
				});
			}
		});
	},
	getId: function (e) {
		if (typeof e.id === 'undefined')
			return e.element.closest('.stencil[data-id]').attr('data-id');
		else
			return this.element.find('.stencil[data-id="' + e.id + ']"').attr('data-id');
	},
	getStencil: function (e) {
		if (typeof e.id === 'undefined')
			return e.element.closest('.stencil[data-id]');
		else
			return this.element.find('.stencil[data-id="' + e.id + '"]');
	},
	isSelected: function (e) {
		return this.getStencil(e).hasClass('ui-selected');
	},
	toggleSelection: function (e) {
		if (this.isSelected(e))
			this.unselect(e);
		else
			this.select(e);
	},
	select: function (e) {
		this.getStencil(e).addClass('ui-selected');
		this.selectionChanged();
	},
	unselectAll: function (e) {
		$('.stencil[data-select="true"]').removeClass('ui-selected');
		this.selectionChanged(e);
	},
	unselect: function (e) {
		this.getStencil(e).removeClass('ui-selected');
		this.selectionChanged();
	},
	selectedStencils: function () {
		return $('.stencil.ui-selected[data-select="true"]');
	},
	selectionChanged: function (e) {
		var instance = this;
		var selection = this.selectedStencils();
		var id = null;

		if (selection.length === 1)
			id = selection.attr('data-id');

		ide.refreshSections({
			sections: 'properties',
			path: ide.selectedPath(),
			data: {
				'mode': 'editStencil',
				'designerSelectionId': id
			},
			onComplete: function (data) {
				instance.syncToolbar();

				if (typeof e !== 'undefined' && $.isFunction(e.onComplete))
					e.onComplete(data);
			}
		});
	},
	syncToolbar: function (e) {
		var selection = this.selectedStencils();

		if (selection.length === 0) {
			this.toolbar().delete.button.prop('disabled', true);
		}
		else {
			this.toolbar().delete.button.prop('disabled', false);
		}
	},
	setupToolbar: function (e) {
		var t = this.toolbar();
		var instance = this;

		t.delete.button.click(function () {
			t.delete.trigger();
		});
	},
	resizeCanvas: function () {
		ide.designerAction({
			'data': {
				'action': 'viewInfo'
			},
			onComplete: function (data) {
				$('#iotDesigner').css({
					'min-width':'100%',
					'width': data.width + 25,
					'height': data.height + 25
				});
			}
		});
	},
	toolbar: function () {
		var instance = this;
		return {
			'delete': {
				'button': $('#btnIoTDelete'),
				'trigger': function () {
					if ($('#btnIoTDelete').prop('disabled') === true)
						return;

					if (!confirm('Are you sure you want to delete selected stencils?'))
						return;

					var items = [];

					$.each(instance.selectedStencils(), function (i, v) {
						items.push(instance.getId({ 'element': $(v) }));
					});

					ide.designerAction({
						'data': {
							'action': 'remove',
							'items': items
						},
						onComplete: function (e) {
							instance.selectedStencils().remove();
							instance.syncToolbar();
							instance.resizeCanvas();
						}
					});
				}
			}
		};
	}
});