using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Management.Items;

namespace TomPIT.Management.ComponentModel
{
	internal class MicroService : IMicroService
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[MaxLength(128)]
		[Required]
		public string Name { get; set; }
		[Browsable(false)]
		public string Url { get; set; }
		[Browsable(false)]
		[KeyProperty]
		public Guid Token { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		public MicroServiceStages SupportedStages { get; set; }
		[Browsable(false)]
		public Guid ResourceGroup { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(ManagementItems.MicroServiceTemplates)]
		public Guid Template { get; set; }
		[Browsable(false)]
		public string Version { get; set; }
		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				 ? base.ToString()
				 : Name;
		}
	}
}