using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;

namespace TomPIT.Compilation.Views
{
	internal class MasterProcessor : ProcessorBase
	{
		private StringBuilder _sb = null;

		public MasterProcessor(IMasterViewConfiguration master, string source) : base(source)
		{
			View = master;
		}

		private IMasterViewConfiguration View { get; }
		public override void Compile(ITenant tenant, IComponent component, IConfiguration configuration)
		{
			AppendBaseType(Builder);
			AddUsings(Builder);
			AddTagHelpers(Builder);

			AppendViewMetaData(Builder, "Master", component.Token);

			Builder.Append(Source);

			var scripts = SelectScripts(tenant, component.MicroService, configuration as IGraphicInterface);

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
