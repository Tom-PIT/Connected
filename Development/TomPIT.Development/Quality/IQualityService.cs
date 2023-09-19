using System;
using System.Collections.Generic;

namespace TomPIT.Development.Quality
{
	[Obsolete]
	public interface IQualityService
	{
		List<IApiTest> Query();
		string SelectBody(Guid identifier);

		void Delete(Guid identifier);
		void Update(Guid identifier, string title, string description, string api, string body, string tags);
		Guid Insert(string title, string description, string api, string body, string tags);
	}
}
