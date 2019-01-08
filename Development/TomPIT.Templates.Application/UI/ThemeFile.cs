using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.UI;

namespace TomPIT.Application.UI
{
	public abstract class ThemeFile : Text, IThemeFile
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		public string Name { get; set; }
	}
}
