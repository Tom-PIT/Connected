using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Features;

namespace TomPIT.Application.Features
{
	[Create("SettingFeature", nameof(Name))]
	public class SettingFeature : Feature, ISettingFeature
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryRouting)]
		[EnvironmentVisibility(Services.EnvironmentMode.Any)]
		public string Value { get; set; }
	}
}
