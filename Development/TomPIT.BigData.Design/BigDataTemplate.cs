using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.BigData.Design
{
	public class BigDataTemplate : MicroServiceTemplate
	{
		public override Guid Token { get { return new Guid("{56FC7471-2B7F-4AFC-80F6-1BFBFD94CCC8}"); } }
		public override string Name { get { return "BigData"; } }
		private static ConcurrentDictionary<string, IItemDescriptor> _items = null;

		static BigDataTemplate()
		{
			_items = new ConcurrentDictionary<string, IItemDescriptor>(new Dictionary<string, IItemDescriptor>{
				{"Api", new ItemDescriptor("Api", "BigDataApi", typeof(Api)) { Category ="Model", Glyph="fal fa-broadcast-tower", Ordinal=100} }
			});
		}

		public override List<IItemDescriptor> ProvideAddItems(IDomElement parent)
		{
			return _items.Values.ToList();
		}
	}
}
