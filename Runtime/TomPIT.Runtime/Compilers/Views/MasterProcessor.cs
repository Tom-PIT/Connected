using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;

namespace TomPIT.Runtime.Compilers.Views
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
		public override void Compile(ISysConnection connection, IComponent component, IConfiguration configuration)
		{
			AppendBaseType(Builder);
			AddUsings(Builder);
			AddTagHelpers(Builder);

			AppendViewMetaData(Builder, "Master", component.Token);

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
