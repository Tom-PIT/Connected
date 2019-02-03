'use strict';

$.widget('tompit.tpIde', {
	options: {
		selection: {
			explorerNode: null,
			view: null
		},
		designer: {
			active: null
		},
		globalization: {
			language: null
		},
		properties: {
			saveMode: 'instant',
			state: [
			],
			title : document.title
		},
		state: [
		]
	},
	_create: function () {
		this.initialize();
	},
	initialize: function () {
		this.initializeExplorer();
	},
	setLanguage: function (value) {
		this.options.globalization.language = value;
	},
	setState: function (property, value) {
		var exists = false;

		$.each(this.options.state, function (i, v) {
			if (v.property === property) {
				v.value = value;
				exists = true;
				return false;
			}
		});

		if (!exists)
			this.options.state.push({
				'property': property,
				'value': value
			});
	},
	getState: function (property) {
		var r = null;

		$.each(this.options.state, function (i, v) {
			if (v.property === property) {
				r = v.value;

				return false;
			}
		});

		return r;
	},
	/*
	 * explorer
	 */
	initializeExplorer: function (e) {
		this._initializeExplorerNodes(e);
		//$(window).keydown(function (e) {
		//	if (e.keyCode === 27) {
		//		$('#devExplorerNodes').sortable('cancel');
		//	}
		//});
	},

	_initializeExplorerNodes: function (e) {
		e = $.extend({
			selector: null,
			path: null,
			mode: 'children'
		}, e);

		var d = null;

		if (e.selector === null)
			d = this.element;
		else {
			var container = null;

			if (e.path !== null)
				container = this._findElement(e.path);

			if (container === null)
				d = e.selector;
			else {
				if (e.mode === 'children')
					d = $(e.selector, container);
				else
					d = container;
			}
		}

		var instance = this;

		$('[data-kind="toggler"]', d).click(function () {
			var target = $(this);
			var icon = target.find('[data-fa-i2svg]');
			var node = target.closest('[data-id]');
			var id = node.attr('data-id');
			var path = instance._resolvePath(target);

			icon.toggleClass('fa-chevron-right')
				.toggleClass('fa-chevron-down');

			var group = node.children('[data-kind="explorer-group"][data-group]');

			if (icon.hasClass('fa-chevron-down')) {
				group.removeClass('collapse');

				var loaded = group.attr('data-loaded');

				if (loaded !== 'true')
					instance._loadExplorerChildren({ path: path });
			}
			else
				group.addClass('collapse');
		});

		$('.dev-explorer-node-text', d).click(function () {
			instance.selectNode({
				target: $(this),
				scroll: false
			});
		});

		var draggables = $('[data-kind="explorer-node"][data-static="false"]', d.parent());

		$.each(draggables, function (i, v) {
			if ($(v).data('draggable'))
				$(v).draggable('destroy');
		});

		var droppables = $('[data-kind="explorer-node"][data-container="true"]', d.parent());

		$.each(droppables, function (i, v) {
			if ($(v).data('droppable'))
				$(v).droppable('destroy');
		});

		$('[data-kind="explorer-node"][data-static="false"]', d.parent()).draggable({
			containment: '#devExplorerNodes',
			scroll: true,
			scrollSpeed: 100,
			cursor: 'move',
			opacity: 0.7,
			revert: true,
			revertDuration: 0,
			create: function (e, ui) {
				instance.options.selection.dragPath = instance._resolvePath($(this));

				$(this).attr('origin', $(this).closest('[data-container="true"]').attr('data-id'));
			}
		});

		$('[data-kind="explorer-node"][data-container="true"]', d.parent()).droppable({
			greedy:true,
			accept: function (e) {
				var att = $(this).attr('data-container');

				if (att === null)
					return false;

				return att === 'true';

				
			},
			drop: function (event, ui) {
				var folder = $(this).attr('data-id');
				var id = ui.draggable.attr('data-id');
				var origin = ui.draggable.attr('origin');
				var path = instance.options.selection.dragPath;

				if (origin === folder)
					return;

				ide.ideAction({
					data: {
						'action': 'move',
						'id': id,
						'folder': folder,
						'path': path
					}, onComplete: function (data) {
						if (origin !== null)
							var originTarget = $('[data-kind="explorer-node"][data-id="' + origin + '"]');

						var target = $('[data-kind="explorer-node"][data-id="' + folder + '"]');

						var originPath = instance._resolvePath(originTarget);
						var targetPath = instance._resolvePath(target);

						if (originPath.indexOf('/') > -1)
							originPath = originPath.substr(0, originPath.lastIndexOf('/'));

						instance.refreshExplorer({ 'path': originPath, 'mode': 'item' });
						$(this).remove();
						instance.refreshExplorer({ 'path': targetPath, 'mode': 'item' });

					}
				});
			}
		});
	},
	
	selectNode: function (e) {
		e = $.extend({
			target: null,
			path:null,
			scroll: true
		}, e);

		if (e.target === null && e.path !== null)
			e.target = this._findElement(e.path);

		var path = this._resolvePath(e.target);

		if (path === this.options.selection.explorerNode) {
			if (typeof e.force === 'undefined' || e.force === false)
				return;
		}

		this.options.selection.explorerNode = path;

		this.draw();

		this.loadSection({
			section: 'designer'
		});

		this.setSelection();

		document.title = e.target.find('span[data-kind="documentName"]').html() + ' (' + this.options.properties.title + ')';

		if (e.scroll)
			this.scrollToNode(e);
	},
	_syncNode: function (s, e) {
		var element = s._findElement(e.target);
		s.options.selection.explorerNode = this._resolvePath(element);

		s.draw();

		s.loadSection({
			section: 'designer'
		});

		this.setSelection();

		if (e.scroll) {
			$('#devExplorer').animate({
				scrollTop: element.offset().top
			}, 1000);
		}
	},
	scrollToNode: function (e) {
		$('#devExplorer').animate({
			scrollTop: e.target.offset().top
		}, 1000);
	},

	setSelection: function (path) {
		this.loadSection({
			section: 'selection',
			path: path
		});
	},
	refreshExplorer: function (e) {
		this._loadExplorerChildren(e);
	},
	expandCurrent: function () {
		this.expand({ path: this.options.selection.explorerNode });
	},
	collapseCurrent: function () {
		var element = this._findElement(this.options.selection.explorerNode);

		if (element === null)
			return;

		var icon = element.children('.dev-explorer-node-content').find('span [data-fa-i2svg]');

		if (icon.length === 0)
			return;

		icon.removeClass('fa-chevron-down')
			.addClass('fa-chevron-right');

		var group = element.children('[data-kind="explorer-group"][data-group]');

		group.addClass('collapse');
	},
	selectNext: function () {
		var element = this._findElement(this.options.selection.explorerNode);

		var group = element.children('[data-kind="explorer-group"][data-group]');

		if (group.length > 0 && !group.hasClass('collapse') && group.children().length > 0) {
			var path = this._resolvePath($('[data-kind="explorer-node"]', group));

			this.selectNode({ target: this._findElement(path) });
		}
		else {
			var next = element.next();

			if (next.length > 0) {
				var p = this._resolvePath(next);
				this.selectNode({ target: this._findElement(p) });
			}
			else {
				var parentNode = element.parent().closest('[data-kind="explorer-node"]');
				var sibling = null;

				while (sibling === null || sibling.length === 0) {
					if (parentNode.length === 0)
						return;

					sibling = parentNode.next();

					if (sibling.length === 0)
						parentNode = parentNode.parent().closest('[data-kind="explorer-node"]');
					else {
						var spath = this._resolvePath(sibling);

						this.selectNode({ target: this._findElement(spath) });
						return;
					}
				}
			}
		}
	},
	selectPrevious: function () {
		var element = this._findElement(this.options.selection.explorerNode);

		if (element.length === 0)
			return;

		var prevSibling = element.prev();

		if (prevSibling.length === 0) {
			prevSibling = element.parent().closest('[data-kind="explorer-node"]');

			if (prevSibling.length === 0)
				return;

			this.selectNode({ target: this._findElement(this._resolvePath(prevSibling)) });
		}
		else {
			var group = prevSibling.children('[data-kind="explorer-group"]');

			while (group.length > 0 && !group.hasClass('collapse') && group.children().length > 0) {
				var nested = group.children('[data-kind="explorer-node"]');

				if (nested.length > 0) {
					var nestedGroup = nested.last().children('[data-kind="explorer-group"]');

					if (nestedGroup.length > 0 && !nestedGroup.hasClass('collapse') && nestedGroup.children().length > 0)
						group = nestedGroup;
					else
						break;
				}
				else
					break;
			}

			if (group.length > 0) {
				var last = group.children('[data-kind="explorer-node"]').last();

				this.selectNode({ target: this._findElement(this._resolvePath(last)) });
			}
			else
				this.selectNode({ target: this._findElement(this._resolvePath(prevSibling)) });
		}
	},
	expand: function (e) {
		e = $.extend({
			path: null,
			onComplete: function () {
				return false;
			}
		}, e);

		var element = this._findElement(e.path);

		if (element.length === 0)
			return;

		var icon = element.children('.dev-explorer-node-content').find('span [data-fa-i2svg]');

		icon.removeClass('fa-chevron-right')
			.addClass('fa-chevron-down');

		var group = element.children('[data-kind="explorer-group"][data-group]');

		group.removeClass('collapse');
		var loaded = group.attr('data-loaded');

		if (loaded !== 'true')
			this._loadExplorerChildren({
				path: e.path,
				onComplete: e.onComplete
			});
		else
			e.onComplete();
	},
	expandTo: function (e) {
		e = $.extend({
			path: null,
			select: true,
			selectedId: null,
			current: null
		}, e);

		var tokens = e.path.split('/');
		var instance = this;

		if (e.current !== null) {
			var currentTokens = e.current.split('/');

			for (var i = 0; i < currentTokens.length; i++)
				tokens.shift();

			if (tokens.length === 0) {
				if (e.select) {
					var fullPath = e.path;

					if (e.selectedId !== null && e.selectedId.length > 0)
						fullPath += '/' + e.selectedId;

					var target = this._findElement(fullPath);
					this.selectNode({
						target: target
					});
				}

				return;
			}
			else
				current = e.current + '/' + tokens.shift();
		}
		else
			var current = tokens.shift();

		instance.expand({
			path: current,
			onComplete: function () {
				instance.expandTo({
					path: e.path,
					select: e.select,
					selectedId: e.selectedId,
					current: current
				});
			}
		});
	},
	_checkSelection: function (e) {
		if (this.options.selection.explorerNode === null)
			return;

		var target = this._findElement(this.options.selection.explorerNode);

		if (target.length === 0) {
			this.options.selection.explorerNode = this.options.selection.explorerNode.substr(0, this.options.selection.explorerNode.lastIndexOf('/'));
			this._checkSelection({ select: true });
		}
		else {
			if (e.select) {
				this.selectNode({ target: target, force:true });
			}
		}
	},
	_loadExplorerChildren: function (e) {
		var url = new tompit.devUrl();
		var instance = this;

		e = $.extend({
			onComplete: function () {
				return false;
			},
			select: false,
			async: true,
			depth: 0,
			mode: 'children'
		}, e);

		tompit.post({
			url: url.environment('dom'),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			async: e.async,
			data: {
				path: e.path,
				depth: e.depth,
				mode: e.mode,
				language: instance.options.globalization.language
			},
			progress: tompit.findProgress(this.element),
			onSuccess: function (data, request) {
				var id = e.path.substr(e.path.lastIndexOf('/') + 1);
				var s = null;

				if (e.mode === 'children') {
					var target = instance._findElement(e.path);
					s = '[data-kind="explorer-group"][data-group="' + id + '"]';
					var group = target.children(s);

					group.attr('data-loaded', 'true');
					group.html(data);
				}
				else {
					var target1 = instance._findElement(e.path);
					s = '[data-kind="explorer-node"][data-id="' + id + '"]';

					$(target1).replaceWith(data);
				}

				instance._initializeExplorerNodes({
					selector: s,
					path: e.path,
					mode: e.mode
				});

				instance.draw();

				if (e.select) {
					instance.expandTo({
						path: e.path,
						select: true
					});
				}

				instance._checkSelection({ select: false });

				e.onComplete(data, request);
			}
		});
	},

	/*
	 * End Explorer
	 */
	draw: function () {
		$('.dev-explorer-node-content.active', this.element).removeClass('active');

		if (this.options.selection.explorerNode !== null) {
			var selected = this._findElement(this.options.selection.explorerNode);

			selected.children('.dev-explorer-node-content').addClass('active');
		}
	},
	saveProperty: function (options) {
		if (this.options.properties.saveMode === 'batch') {
			var exists = false;

			$.each(this.options.properties.state, function (i, v) {
				if (v.property === options.data.property) {
					v.value = options.data.value;
					exists = true;

					return false;
				}
			});

			if (!exists) {
				this.options.properties.state.push({
					property: options.data.property,
					value: options.data.value
				});
			}

			return;
		}

		var url = new tompit.devUrl();
		var instance = this;
		var data = $.extend({
			language: this.options.globalization.language,
			selectionId: this.options.selection.id,
			designerSelectionId: this.options.selection.designerId
		}, options.data);

		tompit.post({
			url: url.environment('save'),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			data: data,
			progress: tompit.findProgress(this.element),
			onError: function (request, status, error) {
				if (typeof options.onError !== 'undefined')
					return options.onError(request, status, error);
			},
			onSuccess: function (data, status, request) {
				if (typeof options.onComplete !== 'undefined')
					options.onComplete(data, request);

				var result = request.getResponseHeader('result');

				if (result !== null && result === '1')
					instance.statusText('<span class="text-info"><i class="fal fa-check-circle"></i> \'' + options.data.property + '\' saved.</span>');
				else
					tompit.warning('Property has not been saved. This is possibly due to the not supported server implementation.', 'Save');

				var invalidate = request.getResponseHeader('invalidate');

				if (invalidate !== null) {
					var path = request.getResponseHeader('path');
					var designerPath = request.getResponseHeader('designerPath');

					instance.refreshSections({
						sections: invalidate,
						path: path,
						designerPath: designerPath
					});
				}

				if (instance.options.designer.active !== null && $.isFunction(instance.options.designer.active.idePropertySaved))
					instance.options.designer.active.idePropertySaved(data);
			}
		});
	},

	refreshDesigner: function () {
		var data = {
			path: this.selectedPath(),
			sections: 'designer'
		};

		this.refreshSections(data);
	},

	refreshSections: function (e) {
		var sec = e.sections.toLowerCase().split(',').map(function (item) {
			return item.trim();
		});

		var explorer = sec.indexOf('explorer') > -1;
		var designer = sec.indexOf('designer') > -1;
		var selection = sec.indexOf('selection') > -1;
		var properties = sec.indexOf('properties') > -1;
		var events = sec.indexOf('events') > -1;
		var toolbox = sec.indexOf('toolbox') > -1;
		var property = sec.indexOf('property') > -1;
		var all = sec.indexOf('all') > -1;

		if (all) {
			this.loadSection({
				section: 'all',
				path: e.path,
				data: e.data,
				onComplete:e.onComplete
			});
		}
		else {
			if (explorer) {
				var path = e.path;
				var depth = 0;

				if (typeof e.explorerPath !== 'undefined') {
					path = e.explorerPath;
					depth: 1;
				}

				this.refreshExplorer({
					path: path,
					depth: depth,
					mode: 'item',
					onComplete: e.onComplete
				});
			}
			if (designer)
				this.loadSection({
					section: 'designer',
					path: e.path,
					data: e.data,
					onComplete: e.onComplete
				});

			if (selection) {
				this.loadSection({
					section: 'selection',
					path: e.path,
					data: e.data,
					onComplete: e.onComplete
				});
			}
			else {
				if (properties)
					this.loadSection({
						section: 'properties',
						path: e.path,
						data: e.data,
						onComplete: e.onComplete
					});
				else {
					if (property)
						this.loadSection({
							section: 'property',
							path: e.path,
							data: e.data,
							onComplete: e.onComplete
						});
				}

				if (events)
					this.loadSection({
						section: 'events',
						path: e.path,
						data: e.data,
						onComplete: e.onComplete
					});

				if (toolbox)
					this.loadSection({
						section: 'toolbox',
						path: e.path,
						data: e.data,
						onComplete: e.onComplete
					});
			}
		}
	},
	/*
	 * property grid
	 */
	clearDescription: function (title, description) {
		$('#devPropertyDescription').empty();
	},

	setDescription: function (title, description) {
		$('#devPropertyDescription').html('<strong>' + title + '</strong><br/>' + description);
	},
	setSelectionId: function (id) {
		this.options.selection.id = id;
	},
	setDesignerSelectionId: function (id) {
		this.options.selection.designerId = id;
	},
	/*
	 * status bar
	 */
	statusText: function (html) {
		$('#devStatus').html(html);
	},

	loadSection: function (e) {
		var url = new tompit.devUrl();
		var instance = this;

		e = $.extend({
			section: null,
			path: null,
			data: null
		}, e);

		if (e.path === null)
			e.path = this.selectedPath();

		var data = $.extend(e.data, {
			path: e.path,
			section: e.section,
			language: instance.options.globalization.language
		});

		tompit.post({
			url: url.environment('Section'),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			data: data,
			progress: tompit.findProgress(this.element),
			onSuccess: function (data, status, request) {
				var s = request.getResponseHeader("section").toLowerCase();

				if (s === 'all') {
					$('#ide').html(data);

					instance.initialize();
				}
				else if (s === 'explorer') {
					$('#devExplorerNodes').html(data);

					instance.initializeExplorer();
				}
				else if (s === 'selection') {
					$('#devSelection').html(data);
				}
				else if (s === 'properties') {
					$('#devTabPropertyGrid').html(data);
				}
				else if (s === 'events') {
					$('#devTabEvents').html(data);
				}
				else if (s === 'toolbox') {
					$('#devTabToolbox').html(data);
				}
				else if (s === 'property') {
					//
				}
				else if (s === 'designer') {
					$('#ide').off('toolbarClick');
					instance.setActiveDesigner(null);
					$('#devDesigner').html(data);
					instance.initializeDesigner();
				}

				if ($.isFunction(e.onComplete))
					e.onComplete(data, request);
			}
		});
	},

	selectedPath: function () {
		var e = this.selectedElement();

		if (e === null || e.length === 0)
			return null;

		return this._resolvePath(e);
	},

	selectedElement: function () {
		if (this.options.selection.explorerNode === null)
			return null;

		return this._findElement(this.options.selection.explorerNode);
	},

	initializeDesigner: function () {
		var instance = this;

		$('[data-tp-tag="toolbar-action"]', this.element).click(function () {
			var e = {
				id: $(this).attr('data-id'),
				cancel: false,
				kind: $(this).attr('data-toolbar-kind'),
				parameters: {

				}
			};

			instance.element.trigger("toolbarClick", e);

			if (!e.cancel)
				instance.designerAction({
					data: $.extend(e.parameters, {
						action: e.id
					})
				});
		});

		tompit.initContainer('#devDesigner');
	},
	setPropertySaveMode: function (v) {
		this.options.properties.saveMode = v;
		this.options.properties.state = [];
	},
	getPropertiesState: function () {
		return this.options.properties.state;
	},
	elementId: function (target) {
		return $(target).closest('[data-id]').attr('data-id');
	},
	setActiveDesigner: function (d) {
		this.options.designer.active = d;
	},
	designerAction: function (d, progress) {
		this._action(d, progress, 'action');
	},
	ideAction: function (d, progress) {
		this._action(d, progress, 'ide');
	},
	_action: function (d, progress, action) {
		var url = new tompit.devUrl();
		var instance = this;
		var path = this.selectedPath();

		if (typeof d !== 'undefined' && typeof d.data !== 'undefined' && typeof d.data.path !== 'undefined')
			path = d.data.path;

		tompit.post({
			url: url.environment(action),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			data: $.extend(d.data, {
				path: path,
				language: instance.options.globalization.language
			}),
			progress: typeof progress === 'undefined' || !progress ? tompit.findProgress(this.element) : null,
			onSuccess: function (data, status, request) {
				var type = request.getResponseHeader('designerResult');

				instance._handleMessage(request);

				if (type === null) {
					if (typeof d.onComplete !== 'undefined')
						d.onComplete(data, request);

					return;
				}

				else if (type === 'partial') {
					if (typeof d.onComplete !== 'undefined')
						d.onComplete(data, request);
				}
				else if (type === 'json') {
					if (typeof d.onComplete !== 'undefined')
						d.onComplete(JSON.parse(data), request);
				}
				else if (type === 'section') {
					var designerPath = request.getResponseHeader('designerPath');
					var path = request.getResponseHeader('path');
					var sections = request.getResponseHeader('invalidate');

					if (sections !== null)
						instance.refreshSections({
							sections: sections,
							path: path,
							explorerPath: designerPath,
							designerPath: designerPath,
							data: data,
							onComplete: function () {
								if ($.isFunction(d.onComplete))
									d.onComplete(data, request);
							}
						});
				}
			}
		});
	},

	_handleMessage: function (request) {
		var kind = request.getResponseHeader('messageKind');

		if (kind === null)
			return;

		var title = request.getResponseHeader('title');
		var message = request.getResponseHeader('message');

		if (title === null && message === null)
			return;

		kind = kind.toLowerCase();

		var explorerPath = request.getResponseHeader('explorerPath');
		var instance = this;

		toastr.clear();

		var options = {
			timeOut: 2500
		};

		if (explorerPath !== null) {
			options.onclick = function () {
				instance.expandTo({
					path: explorerPath
				});
			}
		};

		if (kind === 'primary' || kind === 'information' || kind === 'secondary')
			tompit.info(message, title, options);
		else if (kind === 'success')
			tompit.success(message, title, options);
		else if (kind === 'warning')
			tompit.warning(message, title, options);
		else if (kind === 'danger')
			tompit.danger(message, title, options);
	},

	_resolvePath: function (element) {
		var path = [];
		var current = element.closest('[data-id]');

		while (current.length > 0) {
			path.push(current.attr('data-id'));

			current = current.parent().closest('[data-id]');
		}

		return path.reverse().join('/');
	},

	_findElement: function (path) {
		var tokens = path.split('/');
		var current = null;

		for (var i = 0; i < tokens.length; i++) {
			var container = current;

			if (container === null)
				container = $('#devExplorerNodes');

			current = $('[data-kind="explorer-node"][data-id="' + tokens[i] + '"]', container).first();
		}

		return current;
	},
	/*
	 * toolbar
	 */
	hideToolbar: function () {
		$('#devToolbar').addClass('collapse');
	},

	showToolbar: function () {
		$('#devToolbar').removeClass('collapse');
	},
	/*
	 * selections
	 */
	setSelectionView: function (value) {
		this.options.selection.view = value;
	},

	getSelectionView: function () {
		return this.options.selection.view;
	},
	/*
	 * documents
	 */
	newWindow: function (microService, component, element) {

	},
});

tompit.DEVDEFAULTS = {
	environmentUrl: null
};

tompit.DEVGLOBALIZE = {
	environmentUrlNotSet: null
};

tompit.devUrl = function () {
	this.environment = function (verb) {

		if (tompit.DEVDEFAULTS.environmentUrl === null)
			tompit.clientError(tompit.DEVGLOBALIZE.environmentUrlNotSet);

		if (tompit.DEVDEFAULTS.environmentUrl === '/')
			return tompit.DEVDEFAULTS.environmentUrl + verb;


		return tompit.DEVDEFAULTS.environmentUrl + '/' + verb;
	}
};
