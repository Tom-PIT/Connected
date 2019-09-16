using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Resources;
using TomPIT.MicroServices.Design;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.UI.Theming
{
	[Create(DesignUtils.Stylesheet, nameof(Name))]
	public class FileSystemCssFile : ThemeFile, IStaticResource
	{
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public string VirtualPath { get; set; }

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return string.Format("{0}.css", GetType().ShortName());

			return string.Format("{0}.css", Name);
		}

	}
}
