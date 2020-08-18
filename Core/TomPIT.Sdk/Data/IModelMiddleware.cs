using System.Collections.Generic;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Data
{
	public interface IModelMiddleware<T> : IModelComponent
	{
		List<T> Query([CIP(CIP.ModelQueryOperationProvider)] string operation);
		List<T> Query([CIP(CIP.ModelQueryOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] object e);
		List<T> Query([CIP(CIP.ModelQueryOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] object e, [CIP(CIP.ModelOperationParametersProvider)] object staticArguments);

		T Select([CIP(CIP.ModelQueryOperationProvider)] string operation);
		T Select([CIP(CIP.ModelQueryOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] object e);
		T Select([CIP(CIP.ModelQueryOperationProvider)] string operation, [CIP(CIP.ModelOperationParametersProvider)] object e, [CIP(CIP.ModelOperationParametersProvider)] object staticArguments);

		T CreateEntity(object instance);
		R CreateEntity<R>(object instance);

		T Merge(T entity, object instance);
	}
}
