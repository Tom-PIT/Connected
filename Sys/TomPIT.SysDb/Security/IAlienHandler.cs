using System;
using System.Collections.Generic;
using TomPIT.Globalization;
using TomPIT.Security;

namespace TomPIT.SysDb.Security
{
	public interface IAlienHandler
	{
		void Insert(Guid token, string firstName, string lastName, string email, string mobile, string phone, ILanguage language, string timezone, string resourceType, string resourcePrimaryKey);
		void Update(IAlien alien, string firstName, string lastName, string email, string mobile, string phone, ILanguage language, string timezone, string resourceType, string resourcePrimaryKey);
		void Delete(IAlien alien);

		List<IAlien> Query();
		IAlien Select(Guid token);
	}
}
