using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;

namespace TomPIT.UI
{
	internal class PartialProcessor : ProcessorBase
	{
		private StringBuilder _sb = null;
		private IComponent _component = null;

		public PartialProcessor(IPartialView partial, string source) : base(source)
		{
			View = partial;
		}

		private IPartialView View { get; }
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

			AppendViewMetaData(Builder, "Partial", Component.Token);

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
