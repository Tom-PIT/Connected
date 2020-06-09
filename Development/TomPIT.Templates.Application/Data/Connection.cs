using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Data
{
	[Create(DesignUtils.ComponentConnection)]
	[Syntax(SyntaxAttribute.CSharp)]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	public class Connection : SourceCodeConfiguration, IConnectionConfiguration
	{
		[PropertyEditor(PropertyEditorAttribute.TextArea)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public string Value { get; set; }

		[DefaultValue(true)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		public bool Enabled { get; set; } = true;

		[Items(DesignUtils.DataProviderItems)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public Guid DataProvider { get; set; }
	}
}
