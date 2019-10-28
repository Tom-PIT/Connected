using System.Collections.Generic;
using TomPIT.Security;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareUserDataService
	{
		string Select<A>(A primaryKey);
		string Select<A>(A primaryKey, string topic);
		R Select<R, A>(A primaryKey);
		R Select<R, A>(A primaryKey, string topic);
		List<IUserData> Query(string topic);
		void Update<A, V>(A primaryKey, V value);
		void Update<A, V>(A primaryKey, V value, string topic);
		void Update(List<IUserData> data);

		IUserData Create<A, V>(A primaryKey, V value);
		IUserData Create<A, V>(A primaryKey, V value, string topic);
	}
}
