using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;

namespace TomPIT.ComponentModel
{
	internal class MicroService : IMicroService
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[InvalidateEnvironment(Ide.EnvironmentSection.Explorer | Ide.EnvironmentSection.Designer)]
		[MaxLength(128)]
		[Required]
		public string Name { get; set; }
		[Browsable(false)]
		public string Url { get; set; }
		[Browsable(false)]
		[KeyProperty]
		public Guid Token { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		public MicroServiceStatus Status { get; set; }
		[Browsable(false)]
		public Guid ResourceGroup { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items("TomPIT.Items.MicroServiceTemplatesItems, TomPIT.Management")]
		public Guid Template { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;

		}
	}
}
