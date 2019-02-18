using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Services;

namespace TomPIT.Environment
{
	public class ManagementResourceGroup : IResourceGroup
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[MaxLength(128)]
		[Required]
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public string Name { get; set; }
		[Browsable(false)]
		[KeyProperty]
		public Guid Token { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items("TomPIT.Items.StorageProviderItems, TomPIT.Management")]
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public Guid StorageProvider { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public string ConnectionString { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
