using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel.Diagnostics;
using TomPIT.ComponentModel.IoT;
using TomPIT.Diagnostics;
using TomPIT.MicroServices.IoT.UI.Stencils;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.IoT.UI
{
	[DomDesigner("TomPIT.MicroServices.IoT.Design.Designers.IoTViewDesigner, TomPIT.MicroServices.IoT.Design")]
	[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = EnvironmentMode.Runtime)]
	[DomElement("TomPIT.MicroServices.IoT.Design.Dom.IoTViewElement, TomPIT.MicroServices.IoT.Design")]
	[ViewRenderer("TomPIT.MicroServices.IoT.UI.IoTRenderer, TomPIT.MicroServices.IoT")]
	public class IoTView : ViewBase, IIoTViewConfiguration
	{
		private IMetricOptions _metric = null;
		private ListItems<IIoTElement> _elements = null;

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
		[PropertyCategory(PropertyCategoryAttribute.CategoryBehavior)]
		[DefaultValue(true)]
		public bool Enabled { get; set; } = true;
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items("TomPIT.MicroServices.IoT.Design.Items.IoTHubsItems, TomPIT.MicroServices.IoT.Design")]
		[Required]
		public string Hub { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryAppearance)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(ItemsAttribute.LayoutItems)]
		public string Layout { get; set; }

		[Browsable(false)]
		public IMetricOptions Metrics
		{
			get
			{
				if (_metric == null)
					_metric = new MetricOptions { Parent = this };

				return _metric;
			}
		}
		[Browsable(false)]
		public bool AuthorizationEnabled => false;
	}
}
