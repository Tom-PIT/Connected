using System;
using TomPIT.Annotations;

namespace TomPIT.Services.Context
{
	public interface IContextLocalizationService
	{
		Guid Language { get; }
		string GetString([CodeAnalysisProvider(ExecutionContext.StringTableProvider)]string stringTable, [CodeAnalysisProvider(ExecutionContext.StringTableStringProvider)]string key);
		string GetString([CodeAnalysisProvider(ExecutionContext.StringTableProvider)]string stringTable, [CodeAnalysisProvider(ExecutionContext.StringTableStringProvider)]string key, int lcid);
	}
}
