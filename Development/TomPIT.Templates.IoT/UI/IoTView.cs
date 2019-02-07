using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.IoT.UI.Stencils;

namespace TomPIT.IoT.UI
{
	[DomDesigner("TomPIT.IoT.Designers.IoTViewDesigner, TomPIT.IoT.Design")]
	[DomDesigner(DomDesignerAttribute.PermissionsDesigner, Mode = Services.EnvironmentMode.Runtime)]
	[DomElement("TomPIT.IoT.Dom.IoTViewElement, TomPIT.IoT.Design")]
	[ViewRenderer("TomPIT.IoT.UI.IoTRenderer, TomPIT.IoT")]
	public class IoTView : ComponentConfiguration, IView
	{
		private IMetricConfiguration _metric = null;
		private ListItems<IText> _scripts = null;
		private ListItems<IIoTElement> _elements = null;

		[Browsable(false)]
		public ListItems<IViewHelper> Helpers => null;

		[Items("TomPIT.IoT.Items.ScriptCollection, TomPIT.IoT.Design")]
		public ListItems<IText> Scripts
		{
			get
			{
				if (_scripts == null)
					_scripts = new ListItems<IText> { Parent = this };

				return _scripts;
			}
		}

		[Browsable(false)]
		public Guid TextBlob { get; set; }
		[Browsable(false)]
		public ListItems<IIoTElement> Elements
		{
			get
			{
				if (_elements == null)
					_elements = new ListItems<IIoTElement>();

				return _elements;
			}
		}

		public int Width { get; set; } = 500;
		public int Height { get; set; } = 250;

		public string Url { get; set; }

		[PropertyCategory(PropertyCategoryAttribute.CategoryAppearance)]
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items(ItemsAttribute.LayoutItems)]
		public string Layout { get; set; }

		[Browsable(false)]
		public ListItems<ISnippet> Snippets => null;

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
