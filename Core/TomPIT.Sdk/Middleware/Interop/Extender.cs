using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data;
using TomPIT.Diagnostics;
using TomPIT.Reflection;
using TomPIT.Serialization;

namespace TomPIT.Middleware.Interop
{
	public abstract class Extender<TInput, TOutput> : MiddlewareProxy, IExtender<TInput, TOutput>
	{
		protected Extender()
		{
		}

		public List<TOutput> Invoke(List<TInput> items)
		{
			var obsoleteResult = Extend(items);

			if (obsoleteResult != null)
				return obsoleteResult;

			if (items == null)
				return null;

			Items = items;

			Result = new List<TOutput>();

			if (!Items.Any())
				return Result;

			foreach(var item in Items)
				Extend(item);

			return Result;
		}
		
		protected List<TInput> Items { get; private set; }
		protected List<TOutput> Result { get; private set; }

		protected virtual void OnInvoke(TOutput e)
		{
		}

		[Obsolete("Please use Invoke instead.")]
		public List<TOutput> Extend(List<TInput> items)
		{
			return OnExtend(items);
		}

		[Obsolete("Please use OnInvoke instead.")]
		protected virtual List<TOutput> OnExtend(List<TInput> items)
		{
			return null;
		}

		[Obsolete("Please use Extend instead.")]
		protected virtual TOutput Convert(TInput item)
		{
			var instance = OnCreateInstance();

			if (instance is DataEntity entity && item is DataEntity itemEntity)
				entity.Deserialize(itemEntity);

			return instance;
		}

		private void Extend(TInput item)
		{
			var r = OnExtending();

			if (r == null)
				return;

			Serializer.Populate(item, r);

			if (r is DataEntity entity && item is DataEntity itemEntity)
				entity.Deserialize(itemEntity);

			try
			{
				OnInvoke(r);

				Result.Add(r);
			}
			catch(ExtenderException ex)
			{
				ex.LogError(LogCategories.Middleware);
			}
		}

		protected virtual TOutput OnExtending()
		{
			return (TOutput)typeof(TOutput).CreateInstance();
		}
		[Obsolete("Please use OnExtending instead.")]
		protected virtual TOutput OnCreateInstance()
		{
			return OnExtending();
		}
	}
}
