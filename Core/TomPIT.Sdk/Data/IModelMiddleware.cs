using System.Collections.Generic;

namespace TomPIT.Data
{
	public interface IModelMiddleware<T> : IModelComponent
	{
		List<T> Query(string operation);
		List<T> Query(string operation, object e);

		T Select(string operation);
		T Select(string operation, object e);
	}
}
