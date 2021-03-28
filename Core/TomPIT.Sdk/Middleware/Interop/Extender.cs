using System;
using System.Collections.Generic;
using TomPIT.Data;
using TomPIT.Reflection;

namespace TomPIT.Middleware.Interop
{
	public abstract class Extender<TInput, TOutput> : MiddlewareProxy, IExtender<TInput, TOutput>
	{
		protected Extender()
		{
		}

		public List<TOutput> Invoke(List<TInput> items)
		{
			if (items == null)
				return null;

			return OnExtend(items);
		}
		
		protected virtual List<TOutput> OnInvoke(List<TInput> items)
		{
			return new List<TOutput>();
		}

		[Obsolete("Please use Invoke instead.")]
		public List<TOutput> Extend(List<TInput> items)
		{
			return Invoke(items);
		}

		[Obsolete("Please use OnInvoke instead.")]
		protected virtual List<TOutput> OnExtend(List<TInput> items)
		{
			return OnInvoke(items);
		}

		[Obsolete("Please use Extend instead.")]
		protected virtual TOutput Convert(TInput item)
		{
			return Extend(item);
		}

		protected virtual TOutput Extend(TInput item)
		{
			var r = OnExtend();

			if (r is DataEntity entity && item is DataEntity itemEntity)
				entity.Deserialize(itemEntity);

			return r;
		}
		protected virtual TOutput OnExtend()
		{
			return (TOutput)typeof(TOutput).CreateInstance();
		}

		protected virtual TOutput OnCreateInstance()
		{
			return OnExtend();
		}
	}
}
