using System.Collections.Generic;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.Search
{
	public abstract class SearchMiddleware<T> : MiddlewareComponent, ISearchMiddleware<T>
	{
		public SearchVerb Verb { get; set; } = SearchVerb.Update;
		public virtual SearchValidationBehavior ValidationFailed => SearchValidationBehavior.Complete;

		public T Search(string searchResult)
		{
			return OnSearch(searchResult);
		}

		protected virtual T OnSearch(string searchResult)
		{
			var instance = Serializer.Deserialize<T>(searchResult);

			OnSearch(instance);

			return instance;
		}

		protected virtual void OnSearch(T instance)
		{

		}

		public List<T> Index()
		{
			if (Verb != SearchVerb.Rebuild)
				Validate();

			return OnIndex();
		}

		protected virtual List<T> OnIndex()
		{
			return new List<T>();
		}
	}
}
