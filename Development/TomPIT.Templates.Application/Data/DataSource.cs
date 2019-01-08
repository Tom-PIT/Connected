using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;

namespace TomPIT.Application.Data
{
	[Create("DataSource")]
	[DomDesigner("TomPIT.Application.Design.Designers.DataSourceDesigner, TomPIT.Application.Design")]
	public class DataSource : DataElement, IDataSource
	{
		public const string ComponentCategory = "DataSource";

		private ListItems<IDataField> _fields = null;

		[Items("TomPIT.Application.Design.Items.DataSourceParameterCollection, TomPIT.Application.Design")]
		public override ListItems<IDataParameter> Parameters => base.Parameters;

		[Items("TomPIT.Application.Design.Items.DataFieldCollection, TomPIT.Application.Design")]
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
