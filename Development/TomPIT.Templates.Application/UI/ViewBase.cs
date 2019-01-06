using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Application.UI
{
	public abstract class ViewBase : ComponentConfiguration, IGraphicInterface
	{
		private ListItems<IText> _snippets = null;
		private ListItems<IText> _scripts = null;
		private ListItems<IViewHelper> _helpers = null;

		[Browsable(false)]
		public Guid Id => base.Component;
		[Browsable(false)]
		public IElement Parent => null;
		[Browsable(false)]
		public Guid TextBlob { get; set; }

		[Items("TomPIT.Application.Items.SnippetCollection, TomPIT.Templates.Application")]
		public ListItems<IText> Snippets
		{
			get
			{
				if (_snippets == null)
					_snippets = new ListItems<IText> { Parent = this };

				return _snippets;
			}
		}

		[Items("TomPIT.Application.Items.ScriptCollection, TomPIT.Templates.Application")]
		public ListItems<IText> Scripts
		{
			get
			{
				if (_scripts == null)
					_scripts = new ListItems<IText> { Parent = this };

				return _scripts;
			}
		}

		public void Reset()
		{

		}

		[Items("TomPIT.Application.Items.ViewHelpersCollection, TomPIT.Templates.Application")]
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
