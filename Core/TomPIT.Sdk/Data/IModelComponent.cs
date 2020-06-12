using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Data
{
	public interface IModelComponent : IMiddlewareComponent
	{
		public List<R> Query<R>(string operation);
		public R Select<R>(string operation);
		public R Execute<R>(string operation);

		public List<R> Query<R>(string operation, object e);
		public R Select<R>(string operation, object e);
		public R Execute<R>(string operation, object e);
	}
}
