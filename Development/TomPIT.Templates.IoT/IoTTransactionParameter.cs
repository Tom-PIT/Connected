using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;

namespace TomPIT.IoT
{
	[Create("Parameter", nameof(Name))]
	public class IoTTransactionParameter : ConfigurationElement, IIoTTransactionParameter
	{
		[Required]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Name { get; set; }
		[Required]
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[DefaultValue(DataType.String)]
		public DataType DataType { get; set; } = DataType.String;
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public bool IsNullable { get; set; }

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(Name))
				return base.ToString();

			return Name;
		}
	}
}
