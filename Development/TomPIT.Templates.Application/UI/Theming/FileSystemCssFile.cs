using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Resources;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.UI.Theming
{
	[Create(DesignUtils.Stylesheet, nameof(Name))]
	public class FileSystemCssFile : ThemeFile, IStaticResource
	{
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public string VirtualPath { get; set; }
	}
}
