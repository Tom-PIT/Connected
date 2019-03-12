using System.Collections.Generic;
using TomPIT.ComponentModel.Apis;
using TomPIT.Design.CodeAnalysis.Providers;
using TomPIT.Design.Services;
using TomPIT.Services;

namespace TomPIT.Design
{
	public static class DesignExtensions
	{
		public static List<ISuggestion> DiscoverParameters(this IApiOperation operation, IExecutionContext context)
		{
			return new ApiParameterProvider(context).QueryParameters(operation);
		}
	}
}
