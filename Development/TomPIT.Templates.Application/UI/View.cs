using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.Diagnostics;
using TomPIT.ComponentModel.Messaging;
using TomPIT.ComponentModel.UI;
using TomPIT.Diagnostics;
using TomPIT.Messaging;
using TomPIT.MicroServices.Design;
using TomPIT.Runtime;
using TomPIT.UI;

namespace TomPIT.MicroServices.UI
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[DomElement(DesignUtils.ViewElement)]
	[Syntax(SyntaxAttribute.Razor)]
	public class View : ViewBase, IViewConfiguration
	{
		private IServerEvent _invoke = null;
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

		[EventArguments(typeof(ViewInvokeArguments))]
		public IServerEvent Invoke
		{
			get
			{
				if (_invoke == null)
					_invoke = new ServerEvent { Parent = this };

				return _invoke;
			}
		}
	}
}