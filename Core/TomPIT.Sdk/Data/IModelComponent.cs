using System;
using System.Collections.Generic;
using TomPIT.Middleware;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Data
{
	public enum ConcurrencyMode
	{
		Enabled = 1,
		Disabled = 2
	}
	public interface IModelComponent : IMiddlewareComponent
	{
		public List<R> Query<R>([CIP(CIP.ModelQueryOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] params object[] e);

		public R Select<R>([CIP(CIP.ModelQueryOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] params object[] e);

		int Execute([CIP(CIP.ModelExecuteOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] params object[] e);

		public ConcurrencyMode Concurrency { get; }

		List<Type> QueryEntities();
	}
}
