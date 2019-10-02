using System;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareGlobalizationService
	{
		DateTime FromUtc(DateTime value);
		DateTime ToUtc(DateTime value);

		TimeZoneInfo Timezone { get; }

		Guid Language { get; }
		string GetString([CIP(CIP.StringTableProvider)]string stringTable, [CIP(CIP.StringTableStringProvider)]string key);
		string GetString([CIP(CIP.StringTableProvider)]string stringTable, [CIP(CIP.StringTableStringProvider)]string key, int lcid);
		string TryGetString([CIP(CIP.StringTableProvider)]string stringTable, [CIP(CIP.StringTableStringProvider)]string key);
		string TryGetString([CIP(CIP.StringTableProvider)]string stringTable, [CIP(CIP.StringTableStringProvider)]string key, int lcid);
	}
}
