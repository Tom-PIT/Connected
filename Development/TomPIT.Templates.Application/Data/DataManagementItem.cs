using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;

namespace TomPIT.Application.Data
{
	public abstract class DataManagementItem : Element, IDataManagementItem
	{
		private ListItems<IDataManagementItem> _items = null;

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string Name { get; set; }

		[Items("TomPIT.Application.Design.Items.DataManagementCollection, TomPIT.Application.Design")]
		public ListItems<IDataManagementItem> Items
		{
			get
			{
				if (_items == null)
					_items = new ListItems<IDataManagementItem> { Parent = this };

				return _items;
			}
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
