using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Connectivity;

namespace TomPIT.UI
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
		private IComponent Component
		{
			get
			{
				if (_component == null)
					_component = Instance.GetService<IComponentService>().SelectComponent(Template.Component);

				return _component;
			}
		}

		public override void Compile(ISysConnection connection, IComponent component)
		{
			AppendBaseType(Builder, "TomPIT.UI.MailViewBase");
			AddUsings(Builder);
			AddTagHelpers(Builder);

			AppendViewMetaData(Builder, "MailTemplate", Component.Token);

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
