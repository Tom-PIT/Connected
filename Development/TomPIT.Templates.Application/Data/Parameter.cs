using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;

namespace TomPIT.Application.Data
{
	[Create("Parameter", nameof(Name))]
	public class Parameter : Element, IDataParameter
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string Name { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[DefaultValue(DataType.String)]
		public DataType DataType { get; set; } = DataType.String;

		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		public bool IsNullable { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public bool NullMapping { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryGlobalization)]
		public bool SupportsTimeZone { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? GetType().ShortName() : Name;
		}
	}
}
