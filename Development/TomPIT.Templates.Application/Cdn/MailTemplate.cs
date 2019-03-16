using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Application.Cdn
{
	[DomDesigner("TomPIT.Designers.TextDesigner, TomPIT.Ide")]
	[Syntax(SyntaxAttribute.Razor)]
	public class MailTemplate : ComponentConfiguration, IMailTemplate
	{
		private ListItems<IViewHelper> _helpers = null;

		[Browsable(false)]
		public Guid TextBlob { get; set; }
		[Browsable(false)]
		public ListItems<IText> Scripts => null;

		[Items("TomPIT.Application.Design.Items.ViewHelpersCollection, TomPIT.Application.Design")]
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
