using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Resources
{
	[Create(DesignUtils.Assembly)]
	public class AssemblyFileSystemResource : ComponentConfiguration, IAssemblyFileSystemResource
	{
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public string FileName { get; set; }
	}
}
