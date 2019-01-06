using TomPIT.Annotations;
using TomPIT.ComponentModel.Data;

namespace TomPIT.Application.Data
{
	[Create("BoundField", nameof(Name))]
	public class BoundField : DataField, IBoundField
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		public string Mapping { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryGlobalization)]
		public bool SupportsLocalization { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryGlobalization)]
		public bool SupportsTimeZone { get; set; }
	}
}
