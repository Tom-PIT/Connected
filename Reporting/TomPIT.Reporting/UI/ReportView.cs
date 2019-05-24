using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Reporting.UI
{
	[Create("View")]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = Services.EnvironmentMode.Runtime)]
	[DomElement("TomPIT.Reporting.Design.Dom.ViewElement, TomPIT.Reporting.Design")]
	[Syntax(SyntaxAttribute.Razor)]
	public class ReportView : ViewBase, IView
	{
		private IMetricConfiguration _metric = null;
		public const string ComponentCategory = "View";
		public const string ComponentAuthority = "View";

		[PropertyCategory(PropertyCategoryAttribute.CategoryRouting)]
		public string Url { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryAppearance)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(ItemsAttribute.LayoutItems)]
		public string Layout { get; set; }

		[EnvironmentVisibility(Services.EnvironmentMode.Runtime)]
		public IMetricConfiguration Metrics
		{
			get
			{
				if (_metric == null)
					_metric = new MetricConfiguration { Parent = this };

				return _metric;
			}
		}
	}
}
