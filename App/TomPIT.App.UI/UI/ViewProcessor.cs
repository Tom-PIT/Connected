using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;

namespace TomPIT.UI
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
		private IComponent Component
		{
			get
			{
				if (_component == null)
					_component = Instance.GetService<IComponentService>().SelectComponent(View.Component);

				return _component;
			}
		}

		public override void Compile()
		{
			AppendBaseType(Builder);
			AddUsings(Builder);
			AddTagHelpers(Builder);

			Builder.AppendLine(string.Format("@model {0}", ResolveModel()));
			AppendViewMetaData(Builder, "View", Component.Token);
			Builder.AppendFormat("@{{Layout=\"{0}\";}}", ResolveMaster(View));
			Builder.AppendLine();
			Builder.Append(Source);

			var scripts = Instance.GetService<IViewService>().SelectScripts(Component.MicroService, Component.Token);

			if (!string.IsNullOrWhiteSpace(scripts))
				Builder.AppendFormat("<script>{0}</script>", scripts);
		}

		private string ResolveModel()
		{
			return "TomPIT.Models.IRuntimeViewModel";
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

		private string ResolveMaster(IView view)
		{
			if (string.IsNullOrWhiteSpace(view.Layout))
				return "~/Views/Shared/DefaultMaster.cshtml";

			var tokens = view.Layout.Split(new char[] { '/' }, 2);

			var ms = ((IConfiguration)view).MicroService(Instance.Connection);
			var viewToken = tokens[0];

			if (tokens.Length > 1)
			{
				ms = Instance.Connection.ResolveMicroServiceToken(tokens[0]);
				viewToken = tokens[1];
			}

			var m = Instance.GetService<IComponentService>().SelectComponent(ms, "MasterView", viewToken);

			if (m == null)
				throw new RuntimeException(SR.ErrMasterViewNotFound);

			if (!(Instance.GetService<IViewService>().Select(m.Token) is IMasterView master))
				throw new RuntimeException(SR.ErrMasterViewNotFound);

			var c = Instance.GetService<IComponentService>().SelectComponent(master.Component);

			if (c == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			var s = Instance.GetService<IMicroServiceService>().Select(c.MicroService);

			if (s == null)
				throw new RuntimeException(SR.ErrMicroServiceNotFound);

			var microService = Instance.GetService<IMicroServiceService>().Select(((IConfiguration)view).MicroService(Instance.Connection));

			microService.ValidateMicroServiceReference(Instance.Connection, Instance.Connection.ResolveMicroServiceName(s.Token));

			return string.Format("~/Views/Dynamic/Master/{0}.{1}.cshtml", s.Name, c.Name);
		}
	}
}
