using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;

namespace TomPIT.Application.Data
{
	[Create("Field", nameof(Name))]
	public abstract class DataField : Element, IDataField
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string Name { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[DefaultValue(DataType.String)]
		public DataType DataType { get; set; } = DataType.String;

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? GetType().ShortName() : Name;
		}
	}
}
