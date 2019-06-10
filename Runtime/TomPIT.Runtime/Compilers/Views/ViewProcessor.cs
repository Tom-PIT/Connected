using System.Text;
using Newtonsoft.Json;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;

namespace TomPIT.Runtime.Compilers.Views
{
	internal class ViewProcessor : ProcessorBase
	{
		private StringBuilder _sb = null;
		private IComponent _component = null;

		public ViewProcessor(IView view, string source) : base(source)
		{
			View = view;
		}
		
		private IView View { get; }

		public override void Compile(ISysConnection connection, IComponent component, IConfiguration configuration)
		{
			AppendBaseType(Builder);
			AddUsings(Builder);
			AddTagHelpers(Builder);

			Builder.AppendLine(string.Format("@model {0}", ResolveModel()));
			AppendViewMetaData(Builder, "View", component.Token);
			Builder.AppendFormat("@{{Layout=\"{0}\";}}", ResolveMaster(connection, View));
			Builder.AppendLine();
			Builder.Append(Source);

			var scripts = SelectScripts(connection, component.MicroService, configuration as IGraphicInterface);

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

		private string ResolveMaster(ISysConnection connection, IView view)
		{
			if (string.IsNullOrWhiteSpace(view.Layout))
				return "~/Views/Shared/DefaultMaster.cshtml";

			var tokens = view.Layout.Split(new char[] { '/' }, 2);

			var ms = ((IConfiguration)view).MicroService(connection);
			var viewToken = tokens[0];

			if (tokens.Length > 1)
			{
				ms = connection.ResolveMicroServiceToken(tokens[0]);
				viewToken = tokens[1];
			}

			var m = connection.GetService<IComponentService>().SelectComponent(ms, "MasterView", viewToken);

			if (m == null)
				throw new RuntimeException(SR.ErrMasterViewNotFound);

			if (!(connection.GetService<IComponentService>().SelectConfiguration(m.Token) is IMasterView master))
				throw new RuntimeException(SR.ErrMasterViewNotFound);

			var c = connection.GetService<IComponentService>().SelectComponent(master.Component);

			if (c == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			var s = connection.GetService<IMicroServiceService>().Select(c.MicroService);

			if (s == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			var microService = connection.GetService<IMicroServiceService>().Select(((IConfiguration)view).MicroService(connection));

			microService.ValidateMicroServiceReference(connection, connection.ResolveMicroServiceName(s.Token));

			return string.Format("~/Views/Dynamic/Master/{0}.{1}.cshtml", s.Name, c.Name);
		}
	}
}
