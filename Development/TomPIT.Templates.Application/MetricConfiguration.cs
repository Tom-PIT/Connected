using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Application
{
	internal class MetricConfiguration : Element, IMetricConfiguration
	{
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDiagnostic)]
		public bool Enabled { get; set; }
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDiagnostic)]
		[DefaultValue(MetricLevel.Basic)]
		public MetricLevel Level { get; set; } = MetricLevel.Basic;
	}
}
