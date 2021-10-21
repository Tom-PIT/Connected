using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;

namespace TomPIT.Compilation.Views
{
	internal class PartialProcessor : ProcessorBase
	{
		private StringBuilder _sb = null;

		public PartialProcessor(IPartialViewConfiguration partial, string source) : base(source)
		{
			View = partial;
		}

		private IPartialViewConfiguration View { get; }

		public override void Compile(ITenant tenant, IComponent component, IConfiguration configuration)
		{
			AppendBaseType(Builder);
			AddUsings(Builder);
			AddTagHelpers(Builder);

			AppendViewMetaData(Builder, "Partial", component.Token);

			Builder.Append(Source);
			//var scripts = SelectScripts(tenant, component.MicroService, configuration as IGraphicInterface);

			//if (!string.IsNullOrWhiteSpace(scripts))
			//	Builder.AppendFormat("<script>{0}</script>", scripts);
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
