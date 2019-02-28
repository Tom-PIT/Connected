using System;
using System.Collections.Generic;
using TomPIT.Security;

namespace TomPIT.Data
{
	public interface IUserDataService
	{

		IUserData Select(Guid user, string primaryKey);
		IUserData Select(Guid user, string primaryKey, string topic);
		List<IUserData> Query(Guid user, string topic);

		void Update(Guid user, string primaryKey, string value);
		void Update(Guid user, string primaryKey, string value, string topic);

		void Update(Guid user, List<IUserData> data);
	}
}
