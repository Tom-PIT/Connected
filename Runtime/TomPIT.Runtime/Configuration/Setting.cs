using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Runtime;

namespace TomPIT.Configuration
{
	internal class Setting : ISetting
	{
		[KeyProperty]
		[Browsable(false)]
		public string Name { get; set; }
		[EnvironmentVisibility(EnvironmentMode.Any)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[MaxLength(1024)]
		[InvalidateEnvironment(EnvironmentSection.Designer)]
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
