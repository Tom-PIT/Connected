using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TomPIT.BigData;
using TomPIT.ComponentModel;
using TomPIT.Ide.Collections;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Dom;

namespace TomPIT.MicroServices.BigData.Design
{
	public class BigDataTemplate : MicroServiceTemplate
	{
		public override Guid Token { get { return new Guid("{56FC7471-2B7F-4AFC-80F6-1BFBFD94CCC8}"); } }
		public override string Name { get { return "BigData"; } }
		private static ConcurrentDictionary<string, IItemDescriptor> _items = null;

		static BigDataTemplate()
		{
			_items = new ConcurrentDictionary<string, IItemDescriptor>(new Dictionary<string, IItemDescriptor>
			{
				{ ComponentCategories.BigDataPartition,      new ItemDescriptor("Partition",     ComponentCategories.BigDataPartition,     typeof(Partition)) { Category ="Big Data",   Glyph="fal fa-broadcast-tower",  Ordinal=100}}
			});
		}

		public override List<IItemDescriptor> ProvideGlobalAddItems(IDomElement parent)
		{
			return _items.Values.ToList();
		}

		public override TemplateKind Kind => TemplateKind.Plugin;
	}
}
