using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.MicroServices.Resources
{
	public class NuGetPackageResource : ComponentConfiguration, INuGetPackageResource
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[Required]
		public string Id { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public string Version { get; set; }
	}
}