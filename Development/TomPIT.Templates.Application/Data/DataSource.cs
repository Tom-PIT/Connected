using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;

namespace TomPIT.Application.Data
{
	[Create("DataSource")]
	[DomDesigner("TomPIT.Application.Design.DataSourceDesigner, TomPIT.Templates.Application")]
	public class DataSource : DataElement, IDataSource
	{
		public const string ComponentCategory = "DataSource";

		private ListItems<IDataField> _fields = null;

		[Items("TomPIT.Application.Items.DataSourceParameterCollection, TomPIT.Templates.Application")]
		public override ListItems<IDataParameter> Parameters => base.Parameters;

		[Items("TomPIT.Application.Items.DataFieldCollection, TomPIT.Templates.Application")]
		public ListItems<IDataField> Fields
		{
			get
			{
				if (_fields == null)
					_fields = new ListItems<IDataField> { Parent = this };

				return _fields;
			}
		}
	}
}
