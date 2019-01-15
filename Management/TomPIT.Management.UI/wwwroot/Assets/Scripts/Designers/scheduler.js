'use strict';

$.widget('tompit.tpScheduleDesigner', {
	options: {
		data: {

		},
		text: {

		}
	},
	_create: function () {
		var instance = this;
		debugger;
		$('#editStartTime').dxDateBox({
			type: "time",
			showClearButton: true,
			value: this.options.data.startTime,
			onValueChanged: function () { instance.saveConfiguration();}
		});

		$('#editEndTime').dxDateBox({
			type: "time",
			showClearButton: true,
			value: this.options.data.endTime,
			onValueChanged: function () { instance.saveConfiguration(); }
		});

		$('#editStartDate').dxDateBox({
			type: "date",
			showClearButton: true,
			value: this.options.data.startDate,
			width:'100%',
			onValueChanged: function () { instance.saveConfiguration(); }
		});

		$('#editPattern').dxRadioGroup({
			dataSource: this.options.data.patternItems,
			displayExpr: 'text',
			valueExpr: 'value',
			value: this.options.data.interval,
			onValueChanged: function () {
				var val = $('#editPattern').dxRadioGroup('instance').option('value');

				$('#divPatterns > div').collapse('hide');
				$('#pattern' + val).collapse('show');

				instance.saveConfiguration();
			}
		});

		$('input[name="editPattern"]').change(function () {
			var value = $(this).val();

			$('#divPatterns > div').collapse('hide');
			$('#pattern' + value).collapse('show');
		});

		$('#btnStatus').click(function () {
			ide.designerAction({
				data: {
					action: 'changeStatus',
					status: instance.options.data.changeStatus
				}
			});
		});

		$('[data-kind="editor"]').change(this.saveConfiguration);
		$('input:radio[name=editPattern]').click(this.saveConfiguration);
		$('input:radio[name=dayPattern]').click(this.saveConfiguration);
		$('input:radio[name=monthPattern]').click(this.saveConfiguration);
		$('input:radio[name=yearPattern]').click(this.saveConfiguration);
		$('input:radio[name=editEndDate]').click(this.saveConfiguration);
		$('[data-kind="check"]').click(this.saveConfiguration);
	},

	saveConfiguration: function () {
		debugger;
		var instance = this;
		var data = {};

		data.startTime = $('#editStartTime').dxDateBox('instance').option('value');
		data.endTime = $('#editEndTime').dxDateBox('instance').option('value');
		data.interval = $('#editPattern').dxRadioGroup('instance').option('value');
		data.startDate = $('#editStartDate').dxDateBox('instance').option('value');
		data.endMode = $('input:radio[name=editEndDate]:checked').val();

		switch (data.interval) {
			case instance.options.text.second:
				data.intervalValue = $('#editSecondCount').val();
				break;
			case instance.options.text.minute:
				data.intervalValue = $('#editMinuteCount').val();
				break;
			case instance.options.text.hour:
				data.intervalValue = $('#editHourCount').val();
				break;
			case instance.options.text.day:
				data.dayMode = $('input:radio[name=dayPattern]:checked').val();

				switch (data.dayMode) {
					case "1":
						data.intervalValue = $('#editDayCount').val();
						break;
				}

				break;
			case instance.options.text.week:
				data.intervalValue = $('#editWeekInterval').val();

				var weekdays = 0;

				if ($('#checkWeekMonday').is(':checked'))
					weekdays += 1;

				if ($('#checkWeekTuesday').is(':checked'))
					weekdays += 2;

				if ($('#checkWeekWednesday').is(':checked'))
					weekdays += 4;

				if ($('#checkWeekThursday').is(':checked'))
					weekdays += 8;

				if ($('#checkWeekFriday').is(':checked'))
					weekdays += 16;

				if ($('#checkWeekSaturday').is(':checked'))
					weekdays += 32;

				if ($('#checkWeekSunday').is(':checked'))
					weekdays += 64;

				data.weekdays = weekdays;

				break;
			case instance.options.text.month:
				data.monthMode = $('input:radio[name=dayPattern]:checked').val();

				switch (data.monthMode) {
					case "1":
						data.intervalValue = $('#editMonthCount').val();
						data.monthNumber = $('#editMonthCount2').val();
						break;
					case "2":
						data.intervalCounter = $('#monthPart').val();
						data.monthPart = $('#monthPart2').val();
						data.monthNumber = $('#editMonthCount3').val();
						break;
				}
				break;
			case instance.options.text.year:
				data.intervalValue = $('#editYearInterval').val();
				data.yearMode = data.monthMode = $('input:radio[name=yearPattern]:checked').val();

				switch (data.yearMode) {
					case "1":
						data.monthNumber = $('#editYearPart').val();
						data.dayOfMonth = $('#editYearCount2').val();
						break;
					case "2":
						data.intervalCounter = $('#yearPart').val();
						data.monthPart = $('#yearPart2').val();
						data.monthNumber = $('#editYearMonthPart').val();
						break;
				}
				break;
		}

		switch (data.endMode) {
			case "2":
				data.limit = $('#editEndOccurence').val();
				break;
			case "3":
				data.endDate = $('#editEndByDate').val();
				break;
		}

		data.action = "saveConfiguration";

		ide.designerAction({
			data: data,
			onComplete: function (d) {
				if (typeof d.nextRun !== 'undefined')
					$('#nextRun').html(d.nextRun);
			}
		});
	}
});