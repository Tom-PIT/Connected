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

        $('#editEndByDate').dxDateBox({
            type: "date",
            showClearButton: true,
            value: this.options.data.endDate,
            width: '100%',
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

        $('#btnReset').click(function () {
            ide.designerAction({
                data: {
                    action: 'reset'
                }
            });
        });

        $('#btnRun').click(function () {
            ide.designerAction({
                data: {
                    action: 'run'
                }
            });
        });

        $('#editSecondCount').dxNumberBox({
            value: this.options.data.intervalValue,
            width: '100%',
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#editRetryInterval').dxNumberBox({
            value: this.options.data.retryInterval,
            width: '100%',
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#editDisableTreshold').dxNumberBox({
            value: this.options.data.disableTreshold,
            width: '100%',
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#editMinuteCount').dxNumberBox({
            value: this.options.data.intervalValue,
            width: '100%',
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#editHourCount').dxNumberBox({
            value: this.options.data.intervalValue,
            width: '100%',
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#editWeekInterval').dxNumberBox({
            value: this.options.data.intervalValue,
            width: '100%',
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#dayPatternEach').dxCheckBox({
            value: this.options.data.dayMode === 'EveryNDay' ? true : false,
            text: this.options.text.dayEvery,
            onValueChanged: function (e) {

                if (instance.options.updating)
                    return;

                instance.options.updating = true;
                $('#dayPatternWorkday').dxCheckBox('instance').option('value', !e.value);
                instance.options.updating = false;

                instance.saveConfiguration();
            }
        });

        $('#dayPatternWorkday').dxCheckBox({
            value: this.options.data.dayMode === 'EveryWeekday' ? true : false,
            text: this.options.text.workdays,
            onValueChanged: function (e) {
                if (instance.options.updating)
                    return;

                instance.options.updating = true;
                $('#dayPatternEach').dxCheckBox('instance').option('value', !e.value);
                instance.options.updating = false;

                instance.saveConfiguration();
            }
        });

        $('#editDayCount').dxNumberBox({
            value: this.options.data.intervalValue,
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#editMonthCount').dxNumberBox({
            value: this.options.data.intervalValue,
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#editMonthCount2').dxNumberBox({
            value: this.options.data.monthNumber,
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#editEndOccurence').dxNumberBox({
            value: this.options.data.limit,
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#editMonthCount3').dxNumberBox({
            value: this.options.data.monthNumber,
            width:'100px',
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#editYearCount2').dxNumberBox({
            value: this.options.data.dayOfMonth,
            width: '100px',
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#editYearInterval').dxNumberBox({
            value: this.options.data.intervalValue,
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#yearPatternOn').dxCheckBox({
            value: this.options.data.yearMode === 'ExactDate' ? true : false,
            text: this.options.text.yearOn,
            onValueChanged: function (e) {
                if (instance.options.updating)
                    return;

                instance.options.updating = true;
                $('#yearPatternOnThe').dxCheckBox('instance').option('value', !e.value);
                instance.options.updating = false;

                instance.saveConfiguration();

            }
        });

        $('#yearPatternOnThe').dxCheckBox({
            value: this.options.data.yearMode === 'RelativeDate' ? true : false,
            text: this.options.text.yearOnThe,
            onValueChanged: function (e) {
                if (instance.options.updating)
                    return;

                instance.options.updating = true;
                $('#yearPatternOn').dxCheckBox('instance').option('value', !e.value);
                instance.options.updating = false;

                instance.saveConfiguration();
            }
        });

        $('#checkWeekMonday').dxCheckBox({
            value: this.options.data.weekdays.monday,
            text: this.options.text.monday,
            onValueChanged: function () {instance.saveConfiguration();}
        });

        $('#checkWeekTuesday').dxCheckBox({
            value: this.options.data.weekdays.tuesday,
            text: this.options.text.tuesday,
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#checkWeekWednesday').dxCheckBox({
            value: this.options.data.weekdays.wednesday,
            text: this.options.text.wednesday,
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#checkWeekThursday').dxCheckBox({
            value: this.options.data.weekdays.thursday,
            text: this.options.text.thursday,
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#checkWeekFriday').dxCheckBox({
            value: this.options.data.weekdays.friday,
            text: this.options.text.friday,
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#checkWeekSaturday').dxCheckBox({
            value: this.options.data.weekdays.saturday,
            text: this.options.text.saturday,
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#checkWeekSunday').dxCheckBox({
            value: this.options.data.weekdays.sunday,
            text: this.options.text.sunday,
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#monthPatternEach').dxCheckBox({
            value: this.options.data.monthMode === 'ExactDay' ? true : false,
            text: this.options.text.monthDay,
            onValueChanged: function (e) {
                if (instance.options.updating)
                    return;

                instance.options.updating = true;
                $('#monthPatternThe').dxCheckBox('instance').option('value', !e.value);
                instance.options.updating = false;

                instance.saveConfiguration();
            }
        });

        $('#monthPatternThe').dxCheckBox({
            value: this.options.data.monthMode === 'RelativeDay' ? true : false,
            text: this.options.text.monthThe,
            onValueChanged: function (e) {
                if (instance.options.updating)
                    return;

                instance.options.updating = true;
                $('#monthPatternEach').dxCheckBox('instance').option('value', !e.value);
                instance.options.updating = false;

                instance.saveConfiguration();

            }
        });

        $('#yearPart').dxSelectBox({
            value: this.options.data.intervalCounter,
            dataSource: this.options.data.intervalCounters,
            displayExpr: 'text',
            valueExpr: 'value',
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#monthPart').dxSelectBox({
            value: this.options.data.intervalCounter,
            dataSource: this.options.data.intervalCounters,
            displayExpr: 'text',
            valueExpr:'value',
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#monthPart2').dxSelectBox({
            value: this.options.data.monthPart,
            dataSource: this.options.data.monthParts,
            displayExpr: 'text',
            valueExpr: 'value',
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#yearPart2').dxSelectBox({
            value: this.options.data.monthPart,
            dataSource: this.options.data.monthParts,
            displayExpr: 'text',
            valueExpr: 'value',
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#editYearPart').dxSelectBox({
            value: this.options.data.monthNumber,
            dataSource: this.options.data.monthNumbers,
            displayExpr: 'text',
            valueExpr: 'value',
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#editYearMonthPart').dxSelectBox({
            value: this.options.data.monthNumber,
            dataSource: this.options.data.monthNumbers,
            displayExpr: 'text',
            valueExpr: 'value',
            onValueChanged: function () { instance.saveConfiguration(); }
        });

        $('#editNoEndDate').dxCheckBox({
            value: this.options.data.endMode === 'NoEnd' ? true : false,
            text: this.options.text.noEnd,
            onValueChanged: function (e) {
                if (instance.options.updating)
                    return;

                instance.options.updating = true;
                $('#editEndDateOccurence').dxCheckBox('instance').option('value', !e.value);
                $('#editEndBy').dxCheckBox('instance').option('value', false);
                instance.options.updating = false;

                instance.saveConfiguration();
            }
        });

        $('#editEndDateOccurence').dxCheckBox({
            value: this.options.data.endMode === 'Occurrence' ? true : false,
            text: this.options.text.endAfter,
            onValueChanged: function (e) {
                if (instance.options.updating)
                    return;

                instance.options.updating = true;
                $('#editNoEndDate').dxCheckBox('instance').option('value', !e.value);
                $('#editEndBy').dxCheckBox('instance').option('value', false);
                instance.options.updating = false;

                instance.saveConfiguration();
            }
        });

        $('#editEndBy').dxCheckBox({
            value: this.options.data.endMode === 'Date' ? true : false,
            text: this.options.text.endBy,
            onValueChanged: function (e) {
                if (instance.options.updating)
                    return;

                instance.options.updating = true;
                $('#editNoEndDate').dxCheckBox('instance').option('value', !e.value);
                $('#editEndDateOccurence').dxCheckBox('instance').option('value', false);
                instance.options.updating = false;

                instance.saveConfiguration();
            }
        });

	},

    saveConfiguration: function () {
        if (this.options.updating)
            return;

		var instance = this;
		var data = {};

		data.startTime = $('#editStartTime').dxDateBox('instance').option('value');
		data.endTime = $('#editEndTime').dxDateBox('instance').option('value');
		data.interval = $('#editPattern').dxRadioGroup('instance').option('value');
        data.startDate = $('#editStartDate').dxDateBox('instance').option('value');
        data.retryInterval = $('#editRetryInterval').dxNumberBox('instance').option('value');
        data.disableTreshold = $('#editDisableTreshold').dxNumberBox('instance').option('value');
        data.endMode = $('#editNoEndDate').dxCheckBox('instance').option('value')
            ? 'NoEnd'
            : $('#editEndDateOccurence').dxCheckBox('instance').option('value')
                ? 'Occurrence'
                : 'Date';

        switch (data.interval) {
            case instance.options.text.second:
                data.intervalValue = $('#editSecondCount').dxNumberBox('instance').option('value');
                break;
            case instance.options.text.minute:
                data.intervalValue = $('#editMinuteCount').dxNumberBox('instance').option('value');
                break;
            case instance.options.text.hour:
                data.intervalValue = $('#editHourCount').dxNumberBox('instance').option('value');
                break;
            case instance.options.text.day:
                data.dayMode = $('#dayPatternEach').dxCheckBox('instance').option('value') ? 'EveryNDay' : 'EveryWeekday';

                if (data.dayMode === 'EveryNDay')
                    data.intervalValue = $('#editDayCount').dxNumberBox('instance').option('value');

                break;
            case instance.options.text.week:
                data.intervalValue = $('#editWeekInterval').dxNumberBox('instance').option('value');

                var weekdays = 0;

                if ($('#checkWeekMonday').dxCheckBox('instance').option('value'))
                    weekdays += 1;

                if ($('#checkWeekTuesday').dxCheckBox('instance').option('value'))
                    weekdays += 2;

                if ($('#checkWeekWednesday').dxCheckBox('instance').option('value'))
                    weekdays += 4;

                if ($('#checkWeekThursday').dxCheckBox('instance').option('value'))
                    weekdays += 8;

                if ($('#checkWeekFriday').dxCheckBox('instance').option('value'))
                    weekdays += 16;

                if ($('#checkWeekSaturday').dxCheckBox('instance').option('value'))
                    weekdays += 32;

                if ($('#checkWeekSunday').dxCheckBox('instance').option('value'))
                    weekdays += 64;

                data.weekdays = weekdays;

                break;
            case instance.options.text.month:
                data.monthMode = $('#monthPatternEach').dxCheckBox('instance').option('value') ? 'ExactDay' : 'RelativeDay';

                if (data.monthMode === 'ExactDay') {
                    data.intervalValue = $('#editMonthCount').dxNumberBox('instance').option('value');
                    data.monthNumber = $('#editMonthCount2').dxNumberBox('instance').option('value');
                }
                else {
                    data.intervalCounter = $('#monthPart').dxSelectBox('instance').option('value');
                    data.monthPart = $('#monthPart2').dxSelectBox('instance').option('value');
                    data.monthNumber = $('#editMonthCount3').dxNumberBox('instance').option('value');
                }
                break;
            case instance.options.text.year:
                data.intervalValue = $('#editYearInterval').dxNumberBox('instance').option('value');
                data.yearMode = $('#yearPatternOn').dxCheckBox('instance').option('value') ? 'ExactDate' : 'RelativeDate';

                if (data.yearMode === 'ExactDate') {

                    data.monthNumber = $('#editYearPart').dxSelectBox('instance').option('value');
                    data.dayOfMonth = $('#editYearCount2').dxNumberBox('instance').option('value');
                }
                else {
                    data.intervalCounter = $('#yearPart').dxSelectBox('instance').option('value');
                    data.monthPart = $('#yearPart2').dxSelectBox('instance').option('value');
                    data.monthNumber = $('#editYearMonthPart').dxSelectBox('instance').option('value');
                }

                break;
        }

        if (data.endMode === 'Occurrence') {
            data.limit = $('#editEndOccurence').dxNumberBox('instance').option('value');
        }
        else if (data.endMode === 'Date') {
            data.endDate = $('#editEndByDate').dxDateBox('instance').option('value');
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