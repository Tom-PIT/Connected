using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Services
{
	public abstract class Extender<TInput, TOutput> : IExtender<TInput, TOutput>
	{
		protected Extender(IDataModelContext context)
		{
			Context = context;
		}

		protected IDataModelContext Context { get; }
		public List<TOutput> Extend(List<TInput> items)
		{
			if (items == null)
				return null;

			return OnExtend(items);
		}

		protected abstract List<TOutput> OnExtend(List<TInput> items);
	}
}
