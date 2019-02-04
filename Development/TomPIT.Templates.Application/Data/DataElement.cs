using System;
using System.ComponentModel;
using System.Data;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.ComponentModel.Events;

namespace TomPIT.Application.Data
{
	public abstract class DataElement : ComponentConfiguration, IDataElement
	{
		private ListItems<ComponentModel.Data.IDataParameter> _parameters = null;
		private IServerEvent _preparing = null;
		private IServerEvent _executing = null;
		private IServerEvent _validating = null;
		private IServerEvent _executed = null;
		private IMetricConfiguration _metric = null;

		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[PropertyEditor(PropertyEditorAttribute.TextArea)]
		public string CommandText { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[DefaultValue(CommandType.StoredProcedure)]
		public CommandType CommandType { get; set; } = CommandType.StoredProcedure;

		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[Items("TomPIT.Application.Design.Items.ConnectionItems, TomPIT.Application.Design")]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		public Guid Connection { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[DefaultValue(30)]
		public int CommandTimeout { get; set; } = 30;

		public virtual ListItems<ComponentModel.Data.IDataParameter> Parameters
		{
			get
			{
				if (_parameters == null)
					_parameters = new ListItems<ComponentModel.Data.IDataParameter> { Parent = this };

				return _parameters;
			}
		}

		[EventArguments(typeof(PreparingArguments))]
		public virtual IServerEvent Preparing
		{
			get
			{
				if (_preparing == null)
					_preparing = new ServerEvent { Parent = this };

				return _preparing;
			}

		}

		[EventArguments(typeof(ExecutingArguments))]
		public virtual IServerEvent Executing
		{
			get
			{
				if (_executing == null)
					_executing = new ServerEvent { Parent = this };

				return _executing;
			}

		}

		[EventArguments(typeof(ValidatingArguments))]
		public virtual IServerEvent Validating
		{
			get
			{
				if (_validating == null)
					_validating = new ServerEvent { Parent = this };

				return _validating;
			}
		}

		[EventArguments(typeof(ExecutedArguments))]
		public virtual IServerEvent Executed
		{
			get
			{
				if (_executed == null)
					_executed = new ServerEvent { Parent = this };

				return _executed;
			}
		}

		[EnvironmentVisibility(Services.EnvironmentMode.Runtime)]
		public IMetricConfiguration Metrics
		{
			get
			{
				if (_metric == null)
					_metric = new MetricConfiguration { Parent = this };

				return _metric;
			}
		}
	}
}
