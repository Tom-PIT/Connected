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
using TomPIT.UI;

namespace TomPIT.Runtime.Compilers.Views
{
	internal class ReportProcessor : ProcessorBase
	{
		private StringBuilder _sb = null;

		public ReportProcessor(IReport report) : base(null)
		{
			View = report;
		}

		private IReport View { get; }

		public override void Compile(ISysConnection connection, IComponent component, IConfiguration configuration)
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
