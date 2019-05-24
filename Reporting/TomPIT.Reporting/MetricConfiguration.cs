using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Services;

namespace TomPIT.Reporting
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
