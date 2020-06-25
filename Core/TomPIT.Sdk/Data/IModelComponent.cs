using System.Collections.Generic;
using TomPIT.Middleware;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Data
{
	public interface IModelComponent : IMiddlewareComponent
	{
		public List<R> Query<R>([CIP(CIP.ModelQueryOperationProvider)]string operation);
		public List<R> Query<R>([CIP(CIP.ModelQueryOperationProvider)]string operation, [CIP(CIP.ModelOperationParametersProvider)]object e);

		public R Select<R>([CIP(CIP.ModelQueryOperationProvider)]string operation, [CIP(CIP.ModelOperationParametersProvider)]object e);
		public R Select<R>([CIP(CIP.ModelQueryOperationProvider)]string operation);

		public R Execute<R>([CIP(CIP.ModelExecuteOperationProvider)]string operation, [CIP(CIP.ModelOperationParametersProvider)]object e);
		public R Execute<R>([CIP(CIP.ModelExecuteOperationProvider)]string operation);
		void Execute([CIP(CIP.ModelExecuteOperationProvider)]string operation);
		void Execute([CIP(CIP.ModelExecuteOperationProvider)]string operation, [CIP(CIP.ModelOperationParametersProvider)]object e);
	}
}
