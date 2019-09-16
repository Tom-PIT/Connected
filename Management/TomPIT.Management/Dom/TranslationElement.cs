using TomPIT.Ide.Designers;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Environment;
using TomPIT.Management.Designers;

namespace TomPIT.Management.Dom
{
	internal class TranslationElement : DomElement
	{
		public const string FolderId = "Translations";
		private TranslationsDesigner _designer = null;

		public TranslationElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Id = FolderId;
			Glyph = "fal fa-folder";
			Title = "Translations";
		}

		public override bool HasChildren => false;

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new TranslationsDesigner(this);

				return _designer;
			}
		}
	}
}
