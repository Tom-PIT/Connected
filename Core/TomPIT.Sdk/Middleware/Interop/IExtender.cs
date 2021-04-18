using System;
using System.Collections.Generic;

namespace TomPIT.Middleware.Interop
{
	public interface IExtender<TInput, TOutput>
	{
		[Obsolete("Please use Invoke instead.")]
		List<TOutput> Extend(List<TInput> items);
		List<TOutput> Invoke(List<TInput> items);
	}
}
