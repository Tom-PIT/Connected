using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.Search;
using TomPIT.Services;

namespace TomPIT.Search
{
	public abstract class SearchHandler<T> : ProcessHandler, ISearchHandler<T>
	{
		public SearchHandler(IDataModelContext context) : base(context)
		{
		}

		public SearchVerb Verb { get; set; } = SearchVerb.Update;
		public virtual SearchValidationBehavior ValidationFailed => SearchValidationBehavior.Complete;

		public List<T> Query()
		{
			if (Verb != SearchVerb.Rebuild)
				Validate();

			return OnQuery();
		}

		protected virtual List<T> OnQuery()
		{
			return new List<T>();
		}
	}
}
