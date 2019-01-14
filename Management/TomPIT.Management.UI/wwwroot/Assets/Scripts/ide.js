'use strict';

$.widget('tompit.tpIde', {
	options: {
		selection: {
			explorerNode: null,
			view: null
		},
		documents: {
			open: [
			]
		},
		designer: {
			active:null
		},
		globalization: {
			language: null
		},
		properties: {
			saveMode: 'instant',
			state: [
			]
		},
		state: [
		]
	},
	_create: function () {
		this.initialize();
	},
	initialize: function () {
		this.initializeExplorer();
		this.initializeDocuments();
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
	},

	selectNode: function (e) {
		e = $.extend({
			target: null,
			scroll: true
		}, e);

		var path = this._resolvePath(e.target);

		if (path === this.options.selection.explorerNode)
			return;

		this.options.selection.explorerNode = path;

		this.draw();

		//this.loadSection({
		//	section: 'designer'
		//});

		this.setSelection();

		if (e.scroll)
			this.scrollToNode(e);

		this.activateDocument(this.options.selection.explorerNode);
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

		s.activateDocument(s.options.selection.explorerNode);
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
			onSuccess: function (data) {
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

				if (e.select)
					instance.expandTo({
						path: e.path,
						select: true
					});

				e.onComplete();
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
				data:e.data
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
					mode: 'item'
				});
			}
			if (designer)
				this.loadSection({
					section: 'designer',
					path: e.path,
					data: e.data
				});

			if (selection) {
				this.loadSection({
					section: 'selection',
					path: e.path,
					data: e.data
				});
			}
			else {
				if (properties)
					this.loadSection({
						section: 'properties',
						path: e.path,
						data: e.data
					});
				else {
					if (property)
						this.loadSection({
							section: 'property',
							path: e.path,
							data: e.data
						});
				}

				if (events)
					this.loadSection({
						section: 'events',
						path: e.path,
						data: e.data
					});

				if (toolbox)
					this.loadSection({
						section: 'toolbox',
						path: e.path,
						data: e.data
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
		var url = new tompit.devUrl();
		var instance = this;

		tompit.post({
			url: url.environment('Action'),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			data: $.extend(d.data, {
				path: this.selectedPath(),
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

					if (typeof d.onComplete !== 'undefined')
						d.onComplete(data, request);

					if (sections !== null)
						instance.refreshSections({
							sections: sections,
							path: path,
							explorerPath: designerPath,
							designerPath: designerPath,
							data:data
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
	initializeDocuments: function () {
		var t = $('#divOpenDocs', this.element);
		var instance = this;

		t.dxTabs({
			noDataText: '',
			keyExpr: 'path',
			dataSource: this.options.documents.open,
			itemTemplate: $('#docTemplate'),
			onSelectionChanged: function (e) {
				instance._syncNode(instance, {
					target: e.addedItems[0].path,
					scroll: true
				});
			},
			onItemClick: function (e) {
				var span = $(e.event.target).closest('span');

				if (span.length === 0 || !span.hasClass('doc-control-box'))
					return;

				var kind = span.attr('data-kind');

				if (kind === 'close')
					instance.closeDocument(e.itemData.path);
				else
					instance.pinDocument(e.itemData.path);
			}
		});
	},
	pinDocument: function (path) {
		var widget = this._documentManager();

		widget.beginUpdate();

		var existing = null;

		$.each(this.options.documents.open, function (i, v) {
			if (v.path === path) {
				existing = v;
				return false;
			}
		});

		if (existing === null)
			return;

		existing.pinned = true;

		widget.option('dataSource', this.options.documents.open);
		widget.endUpdate();
	},
	closeDocument: function (path) {
		var existing = null;
		var existingIndex = -1;

		$.each(this.options.documents.open, function (i, v) {
			if (v.path === path) {
				existing = v;
				existingIndex = i;
				return false;
			}
		});

		if (existing !== null)
			this.options.documents.open.splice(existingIndex, 1);

		if (this.options.documents.open.length === 0)
			this.activateDocument(null);
		else {
			if (this.options.documents.open.length >= existingIndex && existingIndex !== 0)
				existingIndex--;

			this.activateDocument(this.options.documents.open[existingIndex].path);
		}
	},
	activeDocument: function () {
		var result = null;

		$.each(this.options.documents.open, function (i, v) {
			if (v.active) {
				result = v;
				return false;
			}
		});

		return result;
	},
	openDocument: function (microService, component, element) {
		var instance = this;

		this.designerAction({
			data: {
				action: 'resolvePath',
				section: 'designer',
				microService: microService,
				component: component,
				element: element
			},
			onComplete: function (data) {
				if (typeof data !== 'undefined' && data !== null)
					instance.expandTo({
						path: data.path,
						select: true
					});
			}
		});
	},
	newWindow: function (microService, component, element) {

	},
	nextDocument: function () {
		if (this.options.documents.open.length === 0)
			return;

		var idx = -1;

		$.each(this.options.documents.open, function (i, v) {
			if (v.active) {
				idx = i;
				return false;
			}
		});

		if (idx === -1)
			return;

		idx++;

		if (this.options.documents.open.length <= idx)
			idx = 0;

		var target = this.options.documents.open[idx];

		this.activateDocument(target.path);
	},
	previousDocument: function () {
		if (this.options.documents.open.length === 0)
			return;

		var idx = -1;

		$.each(this.options.documents.open, function (i, v) {
			if (v.active) {
				idx = i;
				return false;
			}
		});

		if (idx === -1)
			return;

		idx--;

		if (idx < 0)
			idx = this.options.documents.open.length;

		var target = this.options.documents.open[idx];

		this.activateDocument(target.path);
	},
	activateDocument: function (path) {
		var active = this.activeDocument();

		if (active !== null && active.path === path)
			return;

		var widget = this._documentManager();

		widget.beginUpdate();

		var existing = null;

		$.each(this.options.documents.open, function (i, v) { v.active = false; });

		$.each(this.options.documents.open, function (i, v) {
			if (v.path === path) {
				existing = v;
				return false;
			}
		});

		if (existing === null) {
			var node = this._findElement(path);
			var text = node.children('.dev-explorer-node-content')
				.children('.dev-explorer-node-text')
				.children('[data-kind="documentName"]');

			existing = {
				path: path,
				text: text.html(),
				pinned: false,
				active: true
			};

			this.options.documents.open.push(existing);
		}

		existing.active = true;

		var unpinned = this.options.documents.open.filter(function (item) {
			return !item.pinned && !item.active;
		});

		var instance = this;

		$.each(unpinned, function (i, v) {
			var idx = instance.options.documents.open.indexOf(v);

			instance.options.documents.open.splice(idx, 1);
		});

		widget.option('dataSource', this.options.documents.open);
		widget.option('selectedItem', existing);

		widget.endUpdate();

	},
	_documentManager: function () {
		return $('#divOpenDocs', this.element).dxTabs('instance');
	}
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
