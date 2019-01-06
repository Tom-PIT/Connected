using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;

namespace TomPIT.Application.Data
{
	[Create("DataManagement")]
	public class DataManagement : ComponentConfiguration, IDataManagement
	{
		public const string ComponentCategory = "DataManagement";

		private ListItems<IDataManagementItem> _items = null;

		[Items("TomPIT.Application.Items.DataManagementCollection, TomPIT.Templates.Application")]
		public ListItems<IDataManagementItem> Items
		{
			get
			{
				if (_items == null)
					_items = new ListItems<IDataManagementItem> { Parent = this };

				return _items;
			}
		}
	}
}
