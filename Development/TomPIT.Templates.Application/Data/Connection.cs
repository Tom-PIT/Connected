using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;

namespace TomPIT.Application.Data
{
	[Create("Connection")]
	public class Connection : ComponentConfiguration, IConnection
	{
		public const string ComponentCategory = "Connection";

		[PropertyEditor(PropertyEditorAttribute.TextArea)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public string Value { get; set; }

		[DefaultValue(true)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		public bool Enabled { get; set; } = true;

		[Items("TomPIT.Application.Items.DataProviderItems, TomPIT.Application.Design")]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public Guid DataProvider { get; set; }
	}
}
