using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;

namespace TomPIT.Reporting.UI
{
	public abstract class ViewBase : ComponentConfiguration, IGraphicInterface
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

		[Items("TomPIT.Reporting.Design.Items.SnippetCollection, TomPIT.Reporting.Design")]
		public virtual ListItems<ISnippet> Snippets
		{
			get
			{
				if (_snippets == null)
					_snippets = new ListItems<ISnippet> { Parent = this };

				return _snippets;
			}
		}

		[Items("TomPIT.Reporting.Items.ScriptCollection, TomPIT.Reporting.Design")]
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

		[Items("TomPIT.Reporting.Design.Items.ViewHelpersCollection, TomPIT.Reporting.Design")]
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
