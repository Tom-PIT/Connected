using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Application
{
	[DomDesigner(DomDesignerAttribute.MetricDesigner, Mode = EnvironmentMode.Runtime)]
	internal class MetricConfiguration : ConfigurationElement, IMetricConfiguration
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
