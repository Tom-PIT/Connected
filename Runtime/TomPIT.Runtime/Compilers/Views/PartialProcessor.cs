using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Runtime.Compilers.Views
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

		public override void Compile(ISysConnection connection, IComponent component, IConfiguration configuration)
		{
			AppendBaseType(Builder);
			AddUsings(Builder);
			AddTagHelpers(Builder);
			
			AppendViewMetaData(Builder, "Partial", component.Token);

			Builder.Append(Source);

			var scripts = SelectScripts(connection, component.MicroService, configuration as IGraphicInterface);

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
