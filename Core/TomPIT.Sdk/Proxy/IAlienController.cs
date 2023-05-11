using System;
using System.Collections.Immutable;
using TomPIT.Security;

namespace TomPIT.Proxy
{
	public interface IAlienController
	{
		ImmutableList<IAlien> Query();
		IAlien Select(Guid token, string resourceType = null, string resourcePrimaryKey = null, string email = null, string mobile = null, string phone = null);
		Guid Insert(string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone, string resourceType, string resourcePrimaryKey);
		void Update(Guid token, string firstName, string lastName, string email, Guid language, string mobile, string phone, string timezone, string resourceType, string resourcePrimaryKey);
		void Delete(Guid token);
	}
}
