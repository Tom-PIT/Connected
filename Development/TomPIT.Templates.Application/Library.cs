using TomPIT.Annotations;
using TomPIT.ComponentModel;

namespace TomPIT.Application
{
	public class Library : ComponentConfiguration, ILibrary
	{
		public const string ComponentCategory = "Library";

		private ListItems<IText> _scripts = null;

		[Items("TomPIT.Application.Items.LibraryScriptCollection, TomPIT.Templates.Application")]
		public ListItems<IText> Scripts
		{
			get
			{
				if (_scripts == null)
					_scripts = new ListItems<IText> { Parent = this };

				return _scripts;
			}
		}
	}
}
