using System.Collections.Generic;
using TomPIT.Data;
using TomPIT.Reflection;

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

		protected virtual TOutput Convert(TInput item)
		{
			var r = OnCreateInstance();

			if (r is DataEntity entity && item is DataEntity itemEntity)
				entity.Deserialize(itemEntity);

			return r;
		}

		protected virtual TOutput OnCreateInstance()
		{
			return (TOutput)typeof(TOutput).CreateInstance();
		}
	}
}
