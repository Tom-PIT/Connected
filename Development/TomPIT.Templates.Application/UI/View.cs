using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.Diagnostics;
using TomPIT.ComponentModel.UI;
using TomPIT.Diagnostics;
using TomPIT.MicroServices.Design;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.UI
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[DomElement(DesignUtils.ViewElement)]
	[Syntax(SyntaxAttribute.Razor)]
	[ComponentCreatedHandler(DesignUtils.ViewCreateHandler)]
	public class View : ViewBase, IViewConfiguration
	{
		private IMetricOptions _metric = null;

		[PropertyCategory(PropertyCategoryAttribute.CategoryRouting)]
		public string Url { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryAppearance)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(ItemsAttribute.LayoutItems)]
		public string Layout { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[DefaultValue(true)]
		public bool Enabled { get; set; } = true;

		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public IMetricOptions Metrics
		{
			get
			{
				if (_metric == null)
					_metric = new MetricOptions { Parent = this };

				return _metric;
			}
		}

		[DefaultValue(true)]
		public bool AuthorizationEnabled { get; set; } = true;
	}
}