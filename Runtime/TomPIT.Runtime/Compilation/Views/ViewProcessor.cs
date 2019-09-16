using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;
using TomPIT.Exceptions;

namespace TomPIT.Compilation.Views
{
	internal class ViewProcessor : ProcessorBase
	{
		private StringBuilder _sb = null;
		public ViewProcessor(IViewConfiguration view, string source) : base(source)
		{
			View = view;
		}

		private IViewConfiguration View { get; }

		public override void Compile(ITenant tenant, IComponent component, IConfiguration configuration)
		{
			AppendBaseType(Builder);
			AddUsings(Builder);
			AddTagHelpers(Builder);

			Builder.AppendLine(string.Format("@model {0}", ResolveModel()));
			AppendViewMetaData(Builder, "View", component.Token);
			Builder.AppendFormat("@{{Layout=\"{0}\";}}", ResolveMaster(tenant, View));
			Builder.AppendLine();
			Builder.Append(Source);

			var scripts = SelectScripts(tenant, component.MicroService, configuration as IGraphicInterface);

			if (!string.IsNullOrWhiteSpace(scripts))
				Builder.AppendFormat("<script>{0}</script>", scripts);
		}

		private string ResolveModel()
		{
			return "TomPIT.Models.IViewModel";
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

		private string ResolveMaster(ITenant tenant, IViewConfiguration view)
		{
			if (string.IsNullOrWhiteSpace(view.Layout))
				return "~/Views/Shared/DefaultMaster.cshtml";

			var tokens = view.Layout.Split(new char[] { '/' }, 2);

			var ms = ((IConfiguration)view).MicroService();
			var viewToken = tokens[0];

			if (tokens.Length > 1)
			{
				var microSvc = tenant.GetService<IMicroServiceService>().Select(tokens[0]);

				if (microSvc != null)
					ms = microSvc.Token;

				viewToken = tokens[1];
			}

			var m = tenant.GetService<IComponentService>().SelectComponent(ms, "MasterView", viewToken);

			if (m == null)
				throw new RuntimeException(SR.ErrMasterViewNotFound);

			if (!(tenant.GetService<IComponentService>().SelectConfiguration(m.Token) is IMasterViewConfiguration master))
				throw new RuntimeException(SR.ErrMasterViewNotFound);

			var c = tenant.GetService<IComponentService>().SelectComponent(master.Component);

			if (c == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			var s = tenant.GetService<IMicroServiceService>().Select(c.MicroService);

			if (s == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			var microService = tenant.GetService<IMicroServiceService>().Select(((IConfiguration)view).MicroService());

			microService.ValidateMicroServiceReference(s.Name);

			return string.Format("~/Views/Dynamic/Master/{0}.{1}.cshtml", s.Name, c.Name);
		}
	}
}
