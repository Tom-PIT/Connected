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
		public List<R> Query<R>([CIP(CIP.ModelQueryOperationProvider)] string operation);
		public List<R> Query<R>([CIP(CIP.ModelQueryOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] object e);
		public List<R> Query<R>([CIP(CIP.ModelQueryOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] object e, [CIP(CIP.ModelOperationParametersProvider)] object staticArguments);

		public R Select<R>([CIP(CIP.ModelQueryOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] object e);
		public R Select<R>([CIP(CIP.ModelQueryOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] object e, [CIP(CIP.ModelOperationParametersProvider)] object staticArguments);
		public R Select<R>([CIP(CIP.ModelQueryOperationProvider)] string operation);

		int Execute([CIP(CIP.ModelExecuteOperationProvider)] string operation);
		int Execute([CIP(CIP.ModelExecuteOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] object e);
		int Execute([CIP(CIP.ModelExecuteOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] object e, [CIP(CIP.ModelOperationParametersProvider)] object staticArguments);

		public ConcurrencyMode Concurrency { get; }

		List<Type> QueryEntities();
	}
}
