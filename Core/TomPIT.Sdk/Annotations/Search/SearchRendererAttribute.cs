using System;
using TomPIT.Annotations.Design;

namespace TomPIT.Annotations.Search
{
	[AttributeUsage(AttributeTargets.Class)]
	public class SearchRendererAttribute : Attribute
	{
		public SearchRendererAttribute([CodeAnalysisProvider(CodeAnalysisProviderAttribute.PartialProvider)]string partial)
		{
			Partial = partial;
		}

		public string Partial { get; }
	}
}
