using System.Collections.Generic;

namespace TomPIT.Data
{
	public interface IModelMiddleware<T> : IModelComponent
	{
		public List<T> Query(string operation);

		public T Select(string operation);

		public void Execute(string operation);

		public List<T> Query(string operation, object e);

		public T Select(string operation, object e);

		public void Execute(string operation, object e);
	}
}
