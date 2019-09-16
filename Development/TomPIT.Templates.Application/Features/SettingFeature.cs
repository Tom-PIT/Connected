using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.Features;
using TomPIT.MicroServices.Design;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.Features
{
	[Create(DesignUtils.SettingFeature, nameof(Name))]
	public class SettingFeature : Feature, ISettingFeature
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryRouting)]
		[EnvironmentVisibility(EnvironmentMode.Any)]
		public string Value { get; set; }
	}
}
