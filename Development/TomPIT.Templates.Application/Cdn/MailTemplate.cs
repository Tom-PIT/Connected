using System.ComponentModel;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.UI;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Cdn
{
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.Razor)]
	public class MailTemplate : SourceCodeConfiguration, IMailTemplateConfiguration
	{
		private ListItems<IViewHelper> _helpers = null;

		[Browsable(false)]
		public ListItems<IText> Scripts => null;

		[Items(DesignUtils.ViewHelpersItems)]
		public ListItems<IViewHelper> Helpers
		{
			get
			{
				if (_helpers == null)
					_helpers = new ListItems<IViewHelper> { Parent = this };

				return _helpers;
			}
		}
	}
}
