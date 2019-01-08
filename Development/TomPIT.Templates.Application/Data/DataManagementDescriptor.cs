using TomPIT.Annotations;
using TomPIT.ComponentModel.Data;

namespace TomPIT.Application.Data
{
	[Create("Descriptor", nameof(Name))]
	public class DataManagementDescriptor : DataManagementItem, IDataManagementDescriptor
	{
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items("TomPIT.Application.Design.Items.DataSourceItems, TomPIT.Application.Design")]
		public string DataSource { get; set; }
	}
}
