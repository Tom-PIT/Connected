using System.Collections.Generic;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT.Search
{
	public abstract class SearchMiddleware<T> : MiddlewareComponent, ISearchMiddleware<T>
	{
		public SearchVerb Verb { get; set; } = SearchVerb.Update;
		public virtual SearchValidationBehavior ValidationFailed => SearchValidationBehavior.Complete;

		public T Deserialize(string searchResult)
		{
			return OnDeserializeResult(searchResult);
		}

		protected virtual T OnDeserializeResult(string searchResult)
		{
			var instance = Serializer.Deserialize<T>(searchResult);

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
