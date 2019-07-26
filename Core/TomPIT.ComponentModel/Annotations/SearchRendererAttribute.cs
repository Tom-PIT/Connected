using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Services;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	public class SearchRendererAttribute : Attribute
	{
		public SearchRendererAttribute([CodeAnalysisProvider(ExecutionContext.PartialProvider)]string partial)
		{
			Partial = partial;
		}

		public string Partial { get; }
	}
}
