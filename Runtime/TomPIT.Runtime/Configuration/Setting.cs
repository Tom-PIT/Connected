using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;

namespace TomPIT.Configuration
{
	internal class Setting : ISetting
	{
		[KeyProperty]
		[Browsable(false)]
		public string Name { get; set; }
		[EnvironmentVisibility(Services.EnvironmentMode.Any)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[MaxLength(1024)]
		[InvalidateEnvironment(EnvironmentSection.Designer)]
		public string Value { get; set; }
		[Browsable(false)]
		public bool Visible { get; set; }
		[EnvironmentVisibility(Services.EnvironmentMode.Any)]
		[Browsable(false)]
		public DataType DataType { get; set; }
		[EnvironmentVisibility(Services.EnvironmentMode.Any)]
		[MaxLength(256)]
		[PropertyEditor(PropertyEditorAttribute.Tag)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[TagEditor(AllowCustomValues = true)]
		[InvalidateEnvironment(EnvironmentSection.Designer)]
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
