using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Configuration;
using TomPIT.Runtime;

namespace TomPIT.Management.Configuration
{
	internal class Setting : ISetting
	{
		[EnvironmentVisibility(EnvironmentMode.Any)]
		[KeyProperty]
		[InvalidateEnvironment(EnvironmentSection.Designer)]
		[Required]
		[MaxLength(128)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Name { get; set; }
		[EnvironmentVisibility(EnvironmentMode.Any)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[MaxLength(1024)]
		public string Value { get; set; }

		public string Type { get; set; }
		public string PrimaryKey { get; set; }
		public string NameSpace { get; set; }
		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
