using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Quality
{
	public abstract class UnitTestMiddleware : MiddlewareComponent, IUnitTestMiddleware
	{
		public void Initialize()
		{
			OnInitialize();
		}

		protected virtual void OnInitialize()
		{

		}

		public void Invoke(List<IUnitTestMessage> messages)
		{
			OnInvoke(messages);
		}

		protected virtual void OnInvoke(List<IUnitTestMessage> messages)
		{

		}
	}
}
