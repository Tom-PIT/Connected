using System;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;

namespace TomPIT.IoT.UI
{
	public class ViewBase : ComponentConfiguration, IGraphicInterface
	{
		private ListItems<ISnippet> _snippets = null;
		private ListItems<IText> _scripts = null;
		private ListItems<IViewHelper> _helpers = null;

		[Browsable(false)]
		public Guid Id => base.Component;
		[Browsable(false)]
		public IElement Parent => null;
		[Browsable(false)]
		public Guid TextBlob { get; set; }

		[Items("TomPIT.IoT.Design.Items.SnippetCollection, TomPIT.IoT.Design")]
		public virtual ListItems<ISnippet> Snippets
		{
			get
			{
				if (_snippets == null)
					_snippets = new ListItems<ISnippet> { Parent = this };

				return _snippets;
			}
		}

		[Items("TomPIT.IoT.Items.ScriptCollection, TomPIT.IoT.Design")]
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

		[Items("TomPIT.IoT.Design.Items.ViewHelpersCollection, TomPIT.IoT.Design")]
		public virtual ListItems<IViewHelper> Helpers
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
