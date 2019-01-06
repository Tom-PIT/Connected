(function ($) {
	'use strict';

	var tpTable = function (el, options) {
		this.options = options;
		this.container = $(el);
		this.table = $('table', this.container);
		this.pager = $('[data-kind="pager"]', this.container);
		this.body = $('tbody', this.table);

		this.rows = {
			count: 0
		};

		this.paging = {
			current: 0,
			total: 0
		};

		this.searching = {
			typingTimer: null,
			input: null
		};

		this.selection = {
			activeRow: 0
		};

		this.init();
	};

	tpTable.DEFAULTS = {
		paging: {
			enabled: true,
			size: 10
		},
		search: {
			enabled: true,
			selector: null
		},
		selection: {
			enabled: true
		},
		onPageIndexChanged: function (sender, e) {
			return false;
		},
		onSearch: function (sender, e) {
			return false;
		},
		onQuerySearchColumn: function (sender, e) {
			return false;
		},
		onPerformCellSearch: function (sender, e) {
			return false;
		},
		onPerformRowSearch: function (sender, e) {
			return false;
		},
		onRowCountChanged: function (sender, e) {
			return false;
		},
		onActiveRowChanged: function (sender, e) {
			return false;
		}
	};

	tpTable.prototype.init = function () {
		this.initData();
		this.initPaging();
		this.initSearch();
		this.initSelection();
	};

	tpTable.prototype.initData = function () {
		if (this.body.length === 0)
			return;

		var count = this.rows.count;
		this.rows.count = $('tr:not([data-filtered])', this.body).length;

		if (this.rows.count === 0) {
			this.paging.total = 0;
			this.paging.current = 0;
		}
		else {
			this.paging.total = Math.ceil(this.rows.count / this.options.paging.size);

			if (this.paging.current === 0)
				this.paging.current = 1;

			if (this.paging.current > this.paging.total)
				this.paging.current = this.paging.total;
		}

		if (count !== this.rows.count)
			this.options.onRowCountChanged(this, {
				count: this.rows.count
			});
	}

	tpTable.prototype.initPaging = function () {
		if (!this.options.paging.enabled || this.paging.current === 0) {
			this.removePaging();
			return;
		}

		if (this.pager.length == 0)
			return;

		this.pager.empty();

		var content = '<ul class="pagination">';

		//Previous
		content = content.concat('<li class="page-item" data-kind="first"><a class="page-link" href="#"><i class="fa fa-step-backward"></i></a></li>');
		content = content.concat('<li class="page-item" data-kind="prev"><a class="page-link" href="#"><i class="fa fa-chevron-left"></i></a></li>');
		content = content.concat('<li class="page-item" data-kind="next"><a class="page-link" href="#"><i class="fa fa-chevron-right"></i></a></li>');
		content = content.concat('<li class="page-item" data-kind="last"><a class="page-link" href="#"><i class="fa fa-step-forward"></i></a></li>');
		content = content.concat('</ul>');

		this.pager.append(content);

		$('li:not(.disabled)', this.pager).on('click', $.proxy(this.pagerClicked, this));

		this.refreshPager();
	}

	tpTable.prototype.initSelection = function () {
		if (!this.options.selection.enabled)
			return;

		this.table.on('click', $.proxy(function (e) {
			var body = $(e.target).closest('tbody');

			if (body.length == 0)
				return;

			var row = $(e.target).closest('tr');

			if (row.hasClass('table-active'))
				return;

			this.changeActiveRow(row.index());

		}, this));
	};

	tpTable.prototype.changeActiveRow = function (index) {
		var rows = $('tbody tr', this.table);

		rows.removeClass('table-active');

		var ex = this.selection.activeRow;
		this.selection.activeRow = index;

		if (ex != index)
			this.options.onActiveRowChanged(this, {
				row: $(rows.get(index)),
				index: index
			})

		$(rows.get(index)).addClass('table-active');
	}

	tpTable.prototype.pagerClicked = function (e) {
		if (typeof e === 'undefined')
			return;

		var target = $(e.currentTarget);

		if (target.length == 0)
			return;

		var attr = target.closest('li').attr('data-kind');

		if (attr === 'first')
			this.moveFirst();
		else if (attr === 'prev')
			this.movePrevious();
		else if (attr === 'next')
			this.moveNext();
		else if (attr === 'last')
			this.moveLast();
	}

	tpTable.prototype.moveNext = function () {
		this.paging.current++;

		if (this.paging.current > this.paging.total)
			this.paging.current = this.paging.total;

		this.refreshPager();
		this.options.onPageIndexChanged(this, null);
	}

	tpTable.prototype.moveLast = function () {
		this.paging.current = this.paging.total;

		this.refreshPager();
		this.options.onPageIndexChanged(this, null);
	}

	tpTable.prototype.movePrevious = function () {
		this.paging.current--;

		if (this.paging.current < 1)
			this.paging.current = 1;

		this.refreshPager();
		this.options.onPageIndexChanged(this, null);
	}

	tpTable.prototype.moveFirst = function () {
		this.paging.current = 1;

		this.refreshPager();
		this.options.onPageIndexChanged(this, null);
	}

	tpTable.prototype.moveTo = function (index) {
		if (index < 1)
			index = 1;

		if (index > this.paging.total)
			index = this.paging.total;

		this.paging.current = index;

		this.refreshPager();
		this.options.onPageIndexChanged(this, null);
	}

	tpTable.prototype.refreshPager = function () {
		if (this.paging.current < 2) {
			$('[data-kind="prev"]', this.pager).addClass('disabled');
			$('[data-kind="first"]', this.pager).addClass('disabled');
		}
		else {
			$('[data-kind="prev"]', this.pager).removeClass('disabled');
			$('[data-kind="first"]', this.pager).removeClass('disabled');
		}

		if (this.paging.current >= this.paging.total) {
			$('[data-kind="next"]', this.pager).addClass('disabled');
			$('[data-kind="last"]', this.pager).addClass('disabled');
		}
		else {
			$('[data-kind="next"]', this.pager).removeClass('disabled');
			$('[data-kind="last"]', this.pager).removeClass('disabled');
		}

		var min = (this.paging.current * this.options.paging.size) - this.options.paging.size;
		var max = Math.min(min + this.options.paging.size, this.rows.count) - 1;

		var rows = $('tr:not([data-filtered])', this.body);

		for (var i = 0; i < this.rows.count; i++) {
			var row = $(rows.get(i));

			if (i < min || i > max)
				row.addClass('collapse');
			else
				row.removeClass('collapse');
		}
	}

	tpTable.prototype.removePaging = function () {
		if (this.pager.length == 0)
			return;

		this.pager.empty();
		this.pager.collapse();
	}

	tpTable.prototype.initSearch = function () {
		if (!this.options.search.enabled)
			return;

		if (this.options.search.selector == null)
			this.searching.input = $('input[type="search"]', this.container);
		else
			this.searching.input = $(this.options.search.selector);

		if (this.searching.input.length == 0)
			return;

		this.searching.input.on('keyup', $.proxy(this.searchCriteriaChanged, this));
	}

	tpTable.prototype.searchCriteriaChanged = function (e) {
		e.preventDefault();
		e.stopPropagation();

		var input = $(e.currentTarget);

		clearTimeout(this.searching.typingTimer);

		if (e.keyCode == 13) {
			input.blur();
			this.search(this, input.val());
		}
		else if (e.keyCode == 27) {
			input.val('');
			input.blur();
			this.search(this, null);
		}
		else {
			var target = this;

			this.searching.typingTimer = setTimeout(function () {
				target.search(target, input.val());
			}, 750);
		}
	}

	tpTable.prototype.search = function (sender, criteria) {
		var rows = $('tr', sender.body);

		if (typeof criteria === 'undefined' || criteria == null)
			criteria = '';

		var headerCells = $('thead tr th', this.table);
		var instance = this;

		$.each(rows, function (index, value) {
			var row = $(value);

			row.removeClass('collapse');

			if (criteria === '')
				row.removeAttr('data-filtered');
			else {
				var e = {
					row: row,
					result: true
				};

				var found = false;

				if (instance.options.onPerformRowSearch(instance, e)) {
					found = e.result;
				}
				else {
					var cells = $('td', row);

					$.each(cells, function (i, v) {
						var defResult = true;
						var cell = $(v);
						var column = null;

						if (headerCells.length >= i) {
							column = $(headerCells.get(i));
							var ds = column.attr('data-search');

							if (typeof ds !== 'undefined' && ds != false && ds === 'false')
								defResult = false;
						}

						var args = {
							row: row,
							cell: cell,
							column: column,
							index: i,
							result: defResult
						};

						instance.options.onQuerySearchColumn(instance, args);

						if (args.result) {
							var cellText = cell.html();
							var cellValue = cellText;
							var dataSearch = cell.attr('data-search');

							if (typeof dataSearch !== 'undefined' && dataSearch != false)
								cellValue = dataSearch;

							var searchArgs = {
								row: row,
								cell: cell,
								result: true,
								value: cellValue
							};

							if (instance.options.onPerformCellSearch(instance, searchArgs)) {
								found = searchArgs.result;
							}
							else if (cellValue.toLowerCase().indexOf(criteria) >= 0)
								found = true;
						}

						if (found)
							return false;
					});


				}

				if (!found) {
					row.attr('data-filtered', 'true');
				}
				else
					row.removeAttr('data-filtered');
			}
		});

		this.initData();
		this.refreshPager();
		this.options.onSearch(this, {
			criteria: criteria
		});
	}

	tpTable.prototype.getVisibleRows = function () {
		return $('tr:not([data-filtered])', this.body);
	}

	tpTable.prototype.getSelectedRows = function () {
		return $('tr[data-selected]', this.body);
	}

	tpTable.prototype.getUnselectedRows = function () {
		return $('tr:not([data-selected])', this.body);
	}

	tpTable.prototype.selectRow = function (row) {
		$(row).attr('data-selected', 'true');
	}

	tpTable.prototype.unselectRow = function (row) {
		$(row).removeAttr('data-selected');
	}

	tpTable.prototype.isRowSelected = function (row) {
		var attr = $(row).attr('data-selected');

		if (typeof attr === 'undefined' || attr === false)
			return false;

		return attr === 'true';
	}

	tpTable.prototype.isRowFiltered = function (row) {
		var attr = $(row).attr('data-filtered');

		if (typeof attr === 'undefined' || attr === false)
			return false;

		return attr === 'true';
	}


	tpTable.prototype.getNonVisibleRows = function () {
		return $('tr[data-filtered]', this.body);
	}

	$.fn.tpTable = function (options) {
		this.each(function () {
			var target = $(this);
			var data = (target).data('tpTable');
			var defs = $.extend(true, tpTable.DEFAULTS, options);

			if (!data)
				target.data('tpTable', (data = new tpTable(this, defs)));

		});
	}

	$.fn.tpTable.Constructor = tpTable;
})(jQuery);