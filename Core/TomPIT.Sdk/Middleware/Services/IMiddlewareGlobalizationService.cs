using System;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareGlobalizationService
	{
		DateTime FromUtc(DateTime value);
		DateTime ToUtc(DateTime value);

		TimeZoneInfo Timezone { get; }

		Guid Language { get; }
		string GetString([CAP(CAP.StringTableProvider)]string stringTable, [CAP(CAP.StringTableStringProvider)]string key);
		string GetString([CAP(CAP.StringTableProvider)]string stringTable, [CAP(CAP.StringTableStringProvider)]string key, int lcid);
		string TryGetString([CAP(CAP.StringTableProvider)]string stringTable, [CAP(CAP.StringTableStringProvider)]string key);
		string TryGetString([CAP(CAP.StringTableProvider)]string stringTable, [CAP(CAP.StringTableStringProvider)]string key, int lcid);
	}
}
