using System.Collections.Generic;
using TomPIT.Security;

namespace TomPIT.Services.Context
{
	public interface IContextUserDataService
	{
		string Select(string primaryKey);
		string Select(string primaryKey, string topic);
		T Select<T>(string primaryKey);
		T Select<T>(string primaryKey, string topic);
		List<IUserData> Query(string topic);
		void Update(string primaryKey, object value);
		void Update(string primaryKey, object value, string topic);
		void Update(List<IUserData> data);

		IUserData Create(string primaryKey, object value);
		IUserData Create(string primaryKey, object value, string topic);
	}
}
