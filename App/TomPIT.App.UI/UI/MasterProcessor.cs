using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;

namespace TomPIT.UI
{
	internal class MasterProcessor : ProcessorBase
	{
		private StringBuilder _sb = null;
		private IComponent _component = null;

		public MasterProcessor(IMasterView master, string source) : base(source)
		{
			View = master;
		}

		private IMasterView View { get; }
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
			AppendBaseType(Builder);
			AddUsings(Builder);
			AddTagHelpers(Builder);

			AppendViewMetaData(Builder, "Master", Component.Token);

			Builder.Append(Source);

			var scripts = Instance.GetService<IViewService>().SelectScripts(Component.MicroService, Component.Token);

			if (!string.IsNullOrWhiteSpace(scripts))
				Builder.AppendFormat("<script>{0}</script>", scripts);
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
