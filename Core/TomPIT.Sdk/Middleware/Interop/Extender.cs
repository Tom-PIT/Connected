using System.Collections.Generic;

namespace TomPIT.Middleware.Interop
{
	public abstract class Extender<TInput, TOutput> : MiddlewareObject, IExtender<TInput, TOutput>
	{
		protected Extender()
		{
		}

		public List<TOutput> Extend(List<TInput> items)
		{
			if (items == null)
				return null;

			return OnExtend(items);
		}

		protected abstract List<TOutput> OnExtend(List<TInput> items);
	}
}
