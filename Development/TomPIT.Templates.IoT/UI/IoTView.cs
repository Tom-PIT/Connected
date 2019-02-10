using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.ComponentModel.UI;
using TomPIT.IoT.UI.Stencils;

namespace TomPIT.IoT.UI
{
	[DomDesigner("TomPIT.IoT.Designers.IoTViewDesigner, TomPIT.IoT.Design")]
	[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = Services.EnvironmentMode.Runtime)]
	[DomElement("TomPIT.IoT.Dom.IoTViewElement, TomPIT.IoT.Design")]
	[ViewRenderer("TomPIT.IoT.UI.IoTRenderer, TomPIT.IoT")]
	public class IoTView : ViewBase, IIoTView
	{
		private IMetricConfiguration _metric = null;
		private ListItems<IIoTElement> _elements = null;

		[Browsable(false)]
		public override ListItems<IViewHelper> Helpers => null;

		[Browsable(false)]
		public ListItems<IIoTElement> Elements
		{
			get
			{
				if (_elements == null)
					_elements = new ListItems<IIoTElement> { Parent = this };

				return _elements;
			}
		}

		[Browsable(false)]
		public int Width { get; set; } = 500;
		[Browsable(false)]
		public int Height { get; set; } = 250;
		public string Url { get; set; }
		public string Css { get; set; }
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items("TomPIT.IoT.Design.Items.IoTHubsItems, TomPIT.IoT.Design")]
		[Required]
		public string Hub { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryAppearance)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(ItemsAttribute.LayoutItems)]
		public string Layout { get; set; }

		[Browsable(false)]
		public override ListItems<ISnippet> Snippets => null;

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
