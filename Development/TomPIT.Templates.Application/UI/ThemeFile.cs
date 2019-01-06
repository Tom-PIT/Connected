using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;

namespace TomPIT.Application.UI
{
	public abstract class ThemeFile : Text
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		public string Name { get; set; }
	}
}
