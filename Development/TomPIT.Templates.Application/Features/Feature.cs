using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Features;
using TomPIT.Services;

namespace TomPIT.Application.Features
{
	[Create("Feature", nameof(Name))]
	public abstract class Feature : ConfigurationElement, IFeature
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		[InvalidateEnvironment(EnvironmentSection.Explorer| EnvironmentSection.Designer)]
		public string Name { get; set; }

		[EnvironmentVisibility(EnvironmentMode.Any)]
		[DefaultValue(true)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		public bool Enabled { get; set; } = true;

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
