using System;
using System.ComponentModel;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.UI
{
	public abstract class ViewBase : ComponentConfiguration, IGraphicInterface
	{
		private ListItems<ISnippet> _snippets = null;
		private ListItems<IText> _scripts = null;
		private ListItems<IViewHelper> _helpers = null;

		[Browsable(false)]
		public Guid Id => Component;
		[Browsable(false)]
		public IElement Parent => null;
		[Browsable(false)]
		public Guid TextBlob { get; set; }

		[Items(DesignUtils.SnippetItems)]
		public ListItems<ISnippet> Snippets
		{
			get
			{
				if (_snippets == null)
					_snippets = new ListItems<ISnippet> { Parent = this };

				return _snippets;
			}
		}

		[Items(DesignUtils.ScriptItems)]
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
