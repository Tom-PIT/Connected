using System;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Annotations.Search
{
	[AttributeUsage(AttributeTargets.Class)]
	public class SearchRendererAttribute : Attribute
	{
		public SearchRendererAttribute([CIP(CIP.PartialProvider)]string partial)
		{
			Partial = partial;
		}

		public string Partial { get; }
	}
}
