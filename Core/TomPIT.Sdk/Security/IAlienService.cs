using System;

namespace TomPIT.Security
{
	public interface IAlienService
	{
		IAlien Select(Guid token);
		IAlien Select(string email);
		IAlien SelectByMobile(string mobile);
		IAlien SelectByPhone(string phone);
		IAlien Select(string resourceType, string resourcePrimaryKey);
		IAlien Select(string firstName = null, string lastName = null, string email = null, string mobile = null, string phone = null, string resourceType = null, string resourcePrimaryKey = null);
		Guid Insert(string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone, string resourceType = null, string resourcePrimaryKey = null);
		void Update(Guid token, string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone, string resourceType = null, string resourcePrimaryKey = null);
		void Delete(Guid token);
	}
}
