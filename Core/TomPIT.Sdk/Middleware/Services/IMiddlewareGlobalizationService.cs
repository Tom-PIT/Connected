using System;
using System.Globalization;
using AA = TomPIT.Annotations.Design.AnalyzerAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareGlobalizationService
	{
		DateTime FromUtc(DateTime value);
		DateTime ToUtc(DateTime value);

		DateTimeOffset FromUtc(DateTimeOffset value);
		DateTimeOffset ToUtc(DateTimeOffset value);

		TimeZoneInfo Timezone { get; }

		Guid Language { get; }
		CultureInfo Culture { get; }
		string GetString([CIP(CIP.StringTableProvider)][AA(AA.StringTableAnalyzer)] string stringTable, [CIP(CIP.StringTableStringProvider)][AA(AA.StringAnalyzer)] string key);
		string GetString([CIP(CIP.StringTableProvider)][AA(AA.StringTableAnalyzer)] string stringTable, [CIP(CIP.StringTableStringProvider)][AA(AA.StringAnalyzer)] string key, int lcid);
		string TryGetString([CIP(CIP.StringTableProvider)][AA(AA.StringTableAnalyzer)] string stringTable, [CIP(CIP.StringTableStringProvider)][AA(AA.StringAnalyzer)] string key);
		string TryGetString([CIP(CIP.StringTableProvider)][AA(AA.StringTableAnalyzer)] string stringTable, [CIP(CIP.StringTableStringProvider)][AA(AA.StringAnalyzer)] string key, int lcid);

		DateTimeOffset Now { get; }
	}
}
