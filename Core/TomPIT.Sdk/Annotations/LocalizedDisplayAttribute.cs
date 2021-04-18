using System;
using AA = TomPIT.Annotations.Design.AnalyzerAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Annotations
{
	public sealed class LocalizedDisplayAttribute : Attribute
	{
		public LocalizedDisplayAttribute([CIP(CIP.StringTableProvider)][AA(AA.StringTableAnalyzer)] string stringTable, [CIP(CIP.StringTableStringProvider)][AA(AA.StringAnalyzer)] string key)
		{
			StringTable = stringTable;
			Key = key;
		}

		public string StringTable { get; }
		public string Key { get; }
	}
}
