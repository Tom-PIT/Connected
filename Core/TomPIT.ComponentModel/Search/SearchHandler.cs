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

		public T Deserialize(string searchResult)
		{
			return OnDeserializeResult(searchResult);
		}

		protected virtual T OnDeserializeResult(string searchResult)
		{
			var instance = Types.Deserialize<T>(searchResult);

			OnDeserializingResult(instance);

			return instance;
		}

		protected virtual void OnDeserializingResult(T instance)
		{

		}

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
