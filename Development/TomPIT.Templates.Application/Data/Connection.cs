using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Annotations.Design.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Data
{
	[Create(DesignUtils.ComponentConnection)]
	[FileNameExtension("ds")]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[ComponentCreatedHandler("TomPIT.MicroServices.Design.CreateHandlers.Connection, TomPIT.MicroServices.Design")]
	[Syntax(SyntaxAttribute.CSharp)]
	[ClassRequired]
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
