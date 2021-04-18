using System;
using AA = TomPIT.Annotations.Design.AnalyzerAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware
{
	public interface IMiddlewareInterop : IDisposable
	{
		R Invoke<R>([CIP(CIP.ApiOperationProvider)][AA(AA.ApiOperationAnalyzer)]string api);
		dynamic Invoke([CIP(CIP.ApiOperationProvider)][AA(AA.ApiOperationAnalyzer)]string api);

		R Invoke<R, A>([CIP(CIP.ApiOperationProvider)][AA(AA.ApiOperationAnalyzer)]string api, [CIP(CIP.ApiOperationParameterProvider)] A e);

		dynamic Invoke<A>([CIP(CIP.ApiOperationProvider)][AA(AA.ApiOperationAnalyzer)]string api, [CIP(CIP.ApiOperationParameterProvider)] A e);

		R Setting<R>([CIP(CIP.SettingMiddlewareProvider)][AA(AA.ApiOperationAnalyzer)]string middleware, [CIP(CIP.SettingMiddlewareParameterProvider)] string property);
		dynamic Setting([CIP(CIP.SettingMiddlewareProvider)][AA(AA.ApiOperationAnalyzer)]string middleware, [CIP(CIP.SettingMiddlewareParameterProvider)] string property);
	}
}
