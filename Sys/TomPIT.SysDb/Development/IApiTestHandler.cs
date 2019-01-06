using System;
using System.Collections.Generic;

namespace TomPIT.SysDb.Development
{
	public interface IApiTestHandler
	{
		List<IApiTest> Query();
		string SelectBody(Guid identifier);

		void Delete(Guid identifier);
		void Update(Guid identifier, string title, string description, string api, string body, string tags);
		void Insert(Guid identifier, string title, string description, string api, string body, string tags);
	}
}
