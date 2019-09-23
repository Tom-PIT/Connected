using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel.Diagnostics;
using TomPIT.ComponentModel.Messaging;
using TomPIT.ComponentModel.UI;
using TomPIT.Diagnostics;
using TomPIT.Messaging;
using TomPIT.Runtime;
using TomPIT.UI;

namespace TomPIT.MicroServices.Reporting.UI
{
	[Create("View")]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = EnvironmentMode.Runtime)]
	[DomElement("TomPIT.Reporting.Design.Dom.ViewElement, TomPIT.Reporting.Design")]
	[Syntax(SyntaxAttribute.Razor)]
	public class ReportView : ViewBase, IViewConfiguration
	{
		private IServerEvent _invoke = null;
		private IMetricOptions _metric = null;
		public const string ComponentCategory = "View";
		public const string ComponentAuthority = "View";

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
