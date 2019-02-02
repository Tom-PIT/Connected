using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.Security;

namespace TomPIT.Application.UI
{
	[Create("View")]
	[DomDesigner("TomPIT.Designers.TextDesigner, TomPIT.Ide")]
	[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = Services.EnvironmentMode.Runtime)]
	[DomElement("TomPIT.Application.Design.Dom.ViewElement, TomPIT.Application.Design")]
	[Syntax("razor")]
	public class View : ViewBase, IApplicationView
	{
		private IMetricConfiguration _metric = null;
		public const string ComponentCategory = "View";
		public const string ComponentAuthority = "View";

		[PropertyCategory(PropertyCategoryAttribute.CategoryRouting)]
		public string Url { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategoryAppearance)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items("TomPIT.Application.Design.Items.LayoutItems, TomPIT.Application.Design")]
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
