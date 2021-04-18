using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;

namespace TomPIT.Globalization
{
	internal class Language : ILanguage
	{
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryGlobalization)]
		[DefaultValue(0)]
		public int Lcid { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		[MaxLength(64)]
		public string Name { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[DefaultValue(LanguageStatus.Hidden)]
		public LanguageStatus Status { get; set; } = LanguageStatus.Hidden;
		[PropertyCategory(PropertyCategoryAttribute.CategoryGlobalization)]
		[MaxLength(128)]
		public string Mappings { get; set; }
		[Browsable(false)]
		[KeyProperty]
		public Guid Token { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
