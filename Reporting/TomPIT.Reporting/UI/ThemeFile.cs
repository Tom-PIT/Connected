using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.UI;

namespace TomPIT.Reporting.UI
{
	public abstract class ThemeFile : Text, IThemeFile
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		public string Name { get; set; }
	}
}
