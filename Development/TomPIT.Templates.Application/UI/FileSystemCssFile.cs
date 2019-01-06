using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;

namespace TomPIT.Application.UI
{
	[Create("Stylesheet", nameof(Name))]
	public class FileSystemCssFile : ThemeFile
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
