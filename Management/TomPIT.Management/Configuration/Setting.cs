using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;

namespace TomPIT.Configuration
{
	internal class Setting : ISetting
	{
		[EnvironmentVisibility(Services.EnvironmentMode.Any)]
		[KeyProperty]
		[InvalidateEnvironment(EnvironmentSection.Designer)]
		[Required]
		[MaxLength(128)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Name { get; set; }
		[EnvironmentVisibility(Services.EnvironmentMode.Any)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[MaxLength(1024)]
		public string Value { get; set; }
		[Browsable(false)]
		public bool Visible { get; set; }
		[EnvironmentVisibility(Services.EnvironmentMode.Any)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public DataType DataType { get; set; }
		[EnvironmentVisibility(Services.EnvironmentMode.Any)]
		[MaxLength(256)]
		[PropertyEditor(PropertyEditorAttribute.Tag)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[TagEditor(AllowCustomValues = true)]
		public string Tags { get; set; }
		[Browsable(false)]
		public Guid ResourceGroup { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
