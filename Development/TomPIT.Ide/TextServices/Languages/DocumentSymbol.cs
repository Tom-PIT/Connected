using System.Collections.Generic;

namespace TomPIT.Ide.TextServices.Languages
{
	internal class DocumentSymbol : IDocumentSymbol
	{
		private List<IDocumentSymbol> _children = null;
		public List<IDocumentSymbol> Children
		{
			get
			{
				if (_children == null)
					_children = new List<IDocumentSymbol>();

				return _children;
			}
		}

		public string ContainerName { get; set; }

		public string Detail { get; set; }

		public SymbolKind Kind { get; set; }

		public string Name { get; set; }

		public IRange Range { get; set; }

		public IRange SelectionRange { get; set; }

		public List<SymbolTag> Tags { get; set; }
	}
}
