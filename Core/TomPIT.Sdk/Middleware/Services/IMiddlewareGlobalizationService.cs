using System;
using System.Globalization;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareGlobalizationService
	{
		[Obsolete]
		DateTime FromUtc(DateTime value);
		[Obsolete]
		DateTime ToUtc(DateTime value);

		DateTimeOffset FromUtc(DateTimeOffset value);
		DateTimeOffset ToUtc(DateTimeOffset value);

		TimeZoneInfo Timezone { get; }

		Guid Language { get; }
		CultureInfo Culture { get; }
		string GetString([CIP(CIP.StringTableProvider)]string stringTable, [CIP(CIP.StringTableStringProvider)]string key);
		string GetString([CIP(CIP.StringTableProvider)]string stringTable, [CIP(CIP.StringTableStringProvider)]string key, int lcid);
		string TryGetString([CIP(CIP.StringTableProvider)]string stringTable, [CIP(CIP.StringTableStringProvider)]string key);
		string TryGetString([CIP(CIP.StringTableProvider)]string stringTable, [CIP(CIP.StringTableStringProvider)]string key, int lcid);

		DateTimeOffset Now { get; }
	}
}
