using System.Collections.Generic;
using Newtonsoft.Json;

namespace TomPIT.Ide.Analysis.Lenses
{
	internal class CodeLens : ICodeLens
	{
		private List<ICodeLensDescriptor> _items = null;

		[JsonProperty(PropertyName = "items")]
		public List<ICodeLensDescriptor> Items
		{
			get
			{
				if (_items == null)
					_items = new List<ICodeLensDescriptor>();

				return _items;
			}
		}
	}
}
