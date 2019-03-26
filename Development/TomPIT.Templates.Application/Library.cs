using System.Collections.Generic;
using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel;

namespace TomPIT.Application
{
	public class Library : ComponentConfiguration, ILibrary, ISourceCodeContainer
	{
		public const string ComponentCategory = "Library";

		private ListItems<IText> _scripts = null;

		[Items("TomPIT.Application.Design.Items.LibraryScriptCollection, TomPIT.Application.Design")]
		public ListItems<IText> Scripts
		{
			get
			{
				if (_scripts == null)
					_scripts = new ListItems<IText> { Parent = this };

				return _scripts;
			}
		}

		public IText GetReference(string name)
		{
			foreach (CSharpScript i in Scripts)
			{
				if (string.Compare(i.ToString(), name, true) == 0)
					return i;
			}

			return null;
		}

		public List<string> References()
		{
			var r = new List<string>();

			foreach (var i in Scripts)
				r.Add(((CSharpScript)i).ToString());

			return r;
		}

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[DefaultValue(ElementScope.Internal)]
		public ElementScope Scope { get; set; } = ElementScope.Internal;
	}
}
