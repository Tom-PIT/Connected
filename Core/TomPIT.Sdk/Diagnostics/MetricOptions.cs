using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Diagnostics;
using TomPIT.Runtime;

namespace TomPIT.Diagnostics
{
	[DomDesigner(DomDesignerAttribute.MetricDesigner, Mode = EnvironmentMode.Runtime)]
	public class MetricOptions : ConfigurationElement, IMetricOptions
	{
		[Browsable(false)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDiagnostic)]
		public bool Enabled { get; set; }
		[Browsable(false)]
		[PropertyCategory(PropertyCategoryAttribute.CategoryDiagnostic)]
		[DefaultValue(MetricLevel.Basic)]
		public MetricLevel Level { get; set; } = MetricLevel.Basic;
	}
}
