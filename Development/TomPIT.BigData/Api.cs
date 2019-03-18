using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.ComponentModel.Events;

namespace TomPIT.BigData
{
	public class Api : ComponentConfiguration, IBigDataApi
	{
		private IServerEvent _invoke = null;
		private IServerEvent _complete = null;
		private ListItems<ISchemaField> _schema = null;

		[EventArguments(typeof(ApiInvokeArguments))]
		public IServerEvent Invoke
		{
			get
			{
				if (_invoke == null)
					_invoke = new ServerEvent { Parent = this };

				return _invoke;
			}
		}
		[EventArguments(typeof(ApiCompleteArguments))]
		public IServerEvent Complete
		{
			get
			{
				if (_complete == null)
					_complete = new ServerEvent { Parent = this };

				return _complete;
			}
		}
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public string Key { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[DefaultValue(TimestampBehavior.Static)]
		public TimestampBehavior Timestamp { get; set; } = TimestampBehavior.Static;

		[Items("TomPIT.BigData.Design.Items.SchemaFieldsCollection, TomPIT.BigData.Design")]
		public ListItems<ISchemaField> Schema
		{
			get
			{
				if (_schema == null)
					_schema = new ListItems<ISchemaField> { Parent = this };

				return _schema;
			}
		}
	}
}
