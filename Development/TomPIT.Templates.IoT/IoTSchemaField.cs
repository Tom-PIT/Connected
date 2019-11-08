using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;

namespace TomPIT.MicroServices.IoT
{
	[Create("Field", nameof(Name))]
	public class IoTSchemaField : ConfigurationElement, IIoTSchemaField
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[Required]
		public string Name { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public DataType DataType { get; set; } = DataType.String;

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
