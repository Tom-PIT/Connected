using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Services;

namespace TomPIT.ComponentModel
{
	internal class MicroService : IMicroService
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[MaxLength(128)]
		[Required]
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public string Name { get; set; }
		[Browsable(false)]
		public string Url { get; set; }
		[Browsable(false)]
		[KeyProperty]
		public Guid Token { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public MicroServiceStatus Status { get; set; }
		[Browsable(false)]
		public Guid ResourceGroup { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items("TomPIT.Items.MicroServiceTemplatesItems, TomPIT.Management")]
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public Guid Template { get; set; }
		[Browsable(false)]
		public Guid Package { get; set; }
		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;

		}
	}
}
