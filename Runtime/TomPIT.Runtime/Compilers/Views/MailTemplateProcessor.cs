using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Connectivity;

namespace TomPIT.Runtime.Compilers.Views
{
	internal class MailTemplateProcessor : ProcessorBase
	{
		private StringBuilder _sb = null;
		private IComponent _component = null;

		public MailTemplateProcessor(IMailTemplate template, string source) : base(source)
		{
			Template = template;
		}

		private IMailTemplate Template { get; }
		public override void Compile(ISysConnection connection, IComponent component, IConfiguration configuration)
		{
			AppendBaseType(Builder, "TomPIT.UI.MailViewBase");
			AddUsings(Builder);
			AddTagHelpers(Builder);

			AppendViewMetaData(Builder, "MailTemplate", component.Token);

			Builder.Append(Source);
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
