'use strict';

$.widget('tompit.tpEnvironmentUnits', {
	options: {
		selection:null
	},
	_create() {
		this.initContainer($('#unitsPanel'));

		ide.setActiveDesigner(this);

		this.draw();
		this._hookToolbar();

		$('#unitsPanel').sortable({
			items: '.unit-container',
			stop: function (e, ui) {
				var id = ui.item.children('.unit-node').attr('data-id');
				var prev = ui.item.prev().children('.unit-node').attr('data-id');
				var parent = ui.item.closest('.unit-group').attr('group-id');

				ide.designerAction({
					data: {
						'action': 'move',
						'id': id,
						'prev': prev,
						'parent': parent
					}
				});
			}
		});

	},
	initContainer: function (s) {
		var instance = this;

		$('.unit-node', s).click(function (e) {
			instance.select($(this));
			instance.editNode();
		});

		$('.unit-node [data-kind="toggler"]', s).click(function (e) {
			instance.toggle(this, e);
		});
	},
	toggle: function (s, e) {
		e.stopPropagation();

		var container = $(s).closest('.unit-container');
		var icon = $(s).find('[data-fa-i2svg]');

		icon.toggleClass('fa-chevron-right')
			.toggleClass('fa-chevron-down');

		var group = container.children('.unit-group');

		if (icon.hasClass('fa-chevron-down')) {
			group.removeClass('collapse');

			var loaded = group.attr('data-loaded');

			if (loaded !== 'true')
				this.loadChildren({ id: group.attr('group-id') });
		}
		else
			group.addClass('collapse');
	},
	loadChildren: function (d) {
		var instance = this;

		ide.designerAction({
			'data': {
				'action': 'children',
				'id': d.id
			},
			onComplete: function (data) {
				var target = $('[group-id="' + d.id + '"]');
				var content = target.children('.unit-group-content');

				content.html(data);
				target.attr('data-loaded', 'true');

				instance.initContainer(target);

				if ($.isFunction(d.onComplete))
					d.onComplete();
			}
		});
	},
	idePropertySaved: function (data) {
		if (typeof data === 'undefined')
			return;

		if (typeof data.id === 'undefined')
			return;

		var instance = this;
		
		ide.designerAction({
			data: {
				'action': 'refresh',
				'id': data.id
			},
			onComplete(data) {
				var id = $(data).attr('data-id');
				var container = instance.container();

				container.html(data);
				instance.select($('.unit-node[data-id="' + id + '"]:first-child'), true);
				instance.initContainer(instance.container());
			}
		});
		
	},
	select: function (s, force) {
		var selection = s.closest('.unit-node[data-id]');

		if (selection.length === 0)
			this.options.selection = null;
		else {
			if (force || this.options.selection === null)
				this.options.selection = selection;
			else {
				var sid = selection.attr('data-id');
				var eid = this.options.selection.attr('data-id');

				if (sid === eid)
					this.options.selection = null;
				else
					this.options.selection = selection;
			}
		}

		this.draw();
	},
	deselect: function () {
		this.options.selection = null;
		this.draw();

		ide.refreshSections({
			sections: 'properties',
			path: ide.selectedPath()
		});
	},
	draw: function () {
		$('.unit-node').removeClass('selected');

		if (this.options.selection !== null)
			this.options.selection.addClass('selected');

		this.syncToolbar();

		if (this.options.selection !== null) {
			var designer = $('#devDesigner').scrollTop();
			var designerHeight = designer + $('#devDesigner').height();

			var selection = this.options.selection.offset().top;
			var selectionHeight = selection + this.options.selection.height();

			if (!(selectionHeight <= designerHeight && selection >= designer)) {
				$('#devDesigner').animate({
					scrollTop: this.options.selection.offset().top - 150
				}, 1000);
			}
		}
	},
	syncToolbar: function () {
		var addChildButton = $('[data-id="actionAddChild"]');
		var moveTopButton = $('[data-id="actionMoveTop"]');
		var moveBottomButton = $('[data-id="actionMoveBottom"]');
		var moveUpButton = $('[data-id="actionMoveUp"]');
		var moveDownButton = $('[data-id="actionMoveDown"]');
		var nestButton = $('[data-id="actionNest"]');
		var unnestButton = $('[data-id="actionUnnest"]');
		var deleteButton = $('[data-id="actionDelete"]');

		if (this.options.selection === null) {
			this._disableCommand(addChildButton);
			this._disableCommand(moveTopButton);
			this._disableCommand(moveBottomButton);
			this._disableCommand(moveUpButton);
			this._disableCommand(moveDownButton);
			this._disableCommand(nestButton);
			this._disableCommand(unnestButton);
			this._disableCommand(deleteButton);
		}
		else {
			this._enableCommand(addChildButton);

			var s = this.options.selection;

			if (this._isTop(s)) {
				this._disableCommand(moveUpButton);
				this._disableCommand(moveTopButton);
				this._disableCommand(nestButton);
			}
			else {
				this._enableCommand(moveUpButton);
				this._enableCommand(moveTopButton);
				this._enableCommand(nestButton);
			}

			if (this._isBottom(s)) {
				this._disableCommand(moveBottomButton);
				this._disableCommand(moveDownButton);
			}
			else {
				this._enableCommand(moveBottomButton);
				this._enableCommand(moveDownButton);
			}

			if (this._hasParent(s))
				this._enableCommand(unnestButton);
			else
				this._disableCommand(unnestButton);

			this._enableCommand(deleteButton);
		}
	},
	container: function () {
		if (this.options.selection === null)
			return null;

		var r = this.options.selection.closest('.unit-container');

		if (r.length === 0)
			return null;

		return r;
	},
	parent: function () {
		if (this.options.selection === null)
			return null;

		var group = this.options.selection.closest('.unit-group');

		if (group.length === 0)
			return null;

		return group.attr('data-id');
	},
	selectedId: function () {
		if (this.options.selection === null)
			return null;

		return this.options.selection.attr('data-id');
	},
	ordinal: function () {
		if (this.container() === null)
			return 0;

		return this.container().index();
	},
	insertUnit: function (e) {
		var instance = this;
		var ordinal = this.ordinal();
		var parent = this.parent();

		if (this.options.addMode === 'child') {
			parent = this.options.selection.attr('data-id');
			ordinal = 0;
		}

		var data = $.extend({
			'action': 'insert',
			'parent': parent,
			'ordinal': ordinal
		}, e);
		ide.designerAction({
			data: data,
			onComplete: function (data) {
				ide.designerAction({
					data: {
						'action': 'refresh',
						'id': data.id
					},
					onComplete: function (data) {
						var id = $(data).attr('data-id');
						var container = instance.container();
						var skeleton = '<div class="unit-container"></div>';
						var content = $(skeleton).html(data);

						if (container === null)
							$('#unitsPanel').append(content);
						else {
							if (instance.options.addMode === 'child') {
								instance.addToggler(instance.options.selection);
								instance.reloadCurrent({
									onComplete: function () {
										instance.select($('.unit-node[data-id="' + id +'"]'));
										instance.editNode();
									}
								});
								return;
							}
							else
								content.insertAfter(container);
						}

						content = $('.unit-node[data-id="' + id + '"]').parent();

						instance.initContainer(content);
						instance.select(content.children('.unit-node[data-id]'));
						instance.editNode();
						instance.checkEmpty();
					}
				});

			}
		});
	},
	reloadCurrent: function (d) {
		var toggler = this.options.selection.children('[data-kind="toggler"]');
		var group = toggler.closest('.unit-container').children('.unit-group');

		this.collapse(this.options.selection);

		group.attr('data-loaded', 'false');

		this.loadChildren({
			id: group.attr('group-id'),
			onComplete: d.onComplete
		});

		this.expand(this.options.selection);
	},
	expand: function (s) {
		var toggler = s.children('[data-kind="toggler"]');
		var icon = toggler.find('[data-fa-i2svg]');

		icon.removeClass('fa-chevron-right');
		icon.addClass('fa-chevron-down');

		toggler.parent().next('.unit-group').removeClass('collapse');
	},
	collapse: function (s) {
		var toggler = s.children('[data-kind="toggler"]');
		var icon = toggler.find('[data-fa-i2svg]');

		icon.removeClass('fa-chevron-down');
		icon.addClass('fa-chevron-right');

		toggler.parent().next('.unit-group').addClass('collapse');
	},
	addToggler: function (s) {
		var toggler = s.children('[data-kind="toggler"]');
		var id = s.attr('data-id');
		var instance = this;

		if (toggler.length === 0)
			s.prepend('<span data-kind="toggler"><i class="fal fa-chevron-right" data-kind="toggler"></i></span>');

		$('[data-kind="toggler"]', $('.unit-node[data-id="' + id + '"]')).click(function (e) {
			instance.toggle(this, e);
		});
	},
	showAddDialog: function (e) {
		ide.refreshSections({
			sections: 'properties',
			path: ide.selectedPath(),
			data: {
				'mode': 'addUnit'
			}
		});
	},
	editNode: function () {
		ide.refreshSections({
			sections: 'properties',
			path: ide.selectedPath(),
			data: {
				'mode': 'editUnit',
				'id': this.selectedId()
			}
		});
	},
	checkEmpty: function () {
		if ($('#unitsPanel').children('.unit-container').length === 0)
			$('#unitEmpty').collapse('show');
		else
			$('#unitEmpty').collapse('hide');
	},
	_disableCommand: function (c) {
		c.addClass('disabled');
		c.attr('area-disabled', 'true');
		c.attr('tabindex', '-1');
	},
	_enableCommand: function (c) {
		c.removeClass('disabled');
		c.attr('area-disabled', 'false');
		c.removeAttr('tabindex');
	},
	_isTop: function (i) {
		return i.closest('.unit-container').index() === 0;
	},
	_isBottom: function (i) {
		var idx = i.closest('.unit-container').index();

		return idx === i.closest('.unit-container').parent().children().length - 1;
	},
	_hasParent: function (i) {
		return i.closest('.unit-container').parent().closest('.unit-container').length > 0;
	},
	_hookToolbar: function () {
		var instance = this;
		
		$('[data-id="actionAdd"]').click(function (e) {
			instance.options.addMode = 'self';
			instance.showAddDialog();
		});

		$('[data-id="actionAddChild"]').click(function (e) {
			instance.options.addMode = 'child';
			instance.showAddDialog();
		});

		$('[data-id="actionMoveTop"]').click(function (e) {

		});

		$('[data-id="actionMoveBottom"]').click(function (e) {

		});

		$('[data-id="actionMoveUp"]').click(function (e) {

		});

		$('[data-id="actionMoveDown"]').click(function (e) {

		});

		$('[data-id="actionNest"]').click(function (e) {

		});

		$('[data-id="actionUnnest"]').click(function (e) {

		});

		$('[data-id="actionDelete"]').click(function (e) {
			if (instance.options.selection.children('[data-kind="toggler"]').length > 0) {
				tompit.warning('Cannot delete environment unit with children.');
				return;
			}

			$('#removeUnit').modal('show');
		});

		$('#removeUnitConfirm').click(function (e) {
			var id = instance.selectedId();
			var group = instance.options.selection.closest('.unit-group');

			ide.designerAction({
				data: {
					'action': 'delete',
					'id': instance.selectedId()
				},
				onComplete: function () {
					instance.container().remove();
					instance.deselect();

					if (group.length > 0) {
						var items = group.children().children().length;

						if (items === 0) {
							group.prev().children('[data-kind="toggler"]').remove();
							group.attr('data-loaded', 'true');
						}
					}
					instance.checkEmpty();
				}
			});
		});
	}
});