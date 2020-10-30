using System;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Annotations
{
	public sealed class LocalizedCategoryAttribute : Attribute
	{
		public LocalizedCategoryAttribute([CIP(CIP.StringTableProvider)]string stringTable, [CIP(CIP.StringTableStringProvider)]string key)
		{
			StringTable = stringTable;
			Key = key;
		}

		public string StringTable { get; }
		public string Key { get; }
	}
}
