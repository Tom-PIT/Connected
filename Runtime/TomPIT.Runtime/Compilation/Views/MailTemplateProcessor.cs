using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Connectivity;

namespace TomPIT.Compilation.Views
{
	internal class MailTemplateProcessor : ProcessorBase
	{
		private StringBuilder _sb = null;

		public MailTemplateProcessor(IMailTemplateConfiguration template, string source) : base(source)
		{
			Template = template;
		}

		private IMailTemplateConfiguration Template { get; }
		public override void Compile(ITenant tenant, IComponent component, IConfiguration configuration)
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
