using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.Application.Resources
{
	[Create("Assembly")]
	public class AssemblyFileSystemResource : ComponentConfiguration, IAssemblyFileSystemResource
	{
		public const string ComponentCategory = "Assembly";

		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public string FileName { get; set; }
	}
}
