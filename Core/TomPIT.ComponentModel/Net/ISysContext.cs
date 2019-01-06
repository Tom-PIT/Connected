using System;
using Microsoft.IdentityModel.Tokens;
using TomPIT.Caching;

namespace TomPIT.Net
{
	public interface ISysContext
	{
		string Url { get; }

		ISysConnection Connection { get; }
		IMemoryCache Cache { get; }
		T GetService<T>();
		void RegisterService(Type contract, object instance);

		TokenValidationParameters ValidationParameters { get; }
	}
}
