using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Reports;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.UI
{
	internal class ReportProcessor : ProcessorBase
	{
		private StringBuilder _sb = null;
		private IComponent _component = null;

		public ReportProcessor(IReport report) : base(null)
		{
			View = report;
		}

		private IReport View { get; }
		private IComponent Component
		{
			get
			{
				if (_component == null)
					_component = Instance.GetService<IComponentService>().SelectComponent(View.Component);

				return _component;
			}
		}

		public override void Compile(ISysConnection connection, IComponent component)
		{
			var renderer = View.GetType().FindAttribute<ViewRendererAttribute>();

			if (renderer == null)
				return;

			var irenderer = renderer.Type != null
				? renderer.Type.CreateInstance<IViewRenderer>()
				: Type.GetType(renderer.TypeName).CreateInstance<IViewRenderer>();

			if (irenderer == null)
				return;

			Builder.Append(irenderer.CreateContent(connection, component));
		}

		public override string Result { get { return Builder.ToString(); } }

		private StringBuilder Builder
		{
			get
			{
				if (_sb == null)
					_sb = new StringBuilder();

				return _sb;
			}
		}
	}
}
