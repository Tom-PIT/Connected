using System.Collections.Generic;

namespace TomPIT.Middleware.Interop
{
	public interface IExtender<TInput, TOutput>
	{
		List<TOutput> Extend(List<TInput> items);
	}
}
