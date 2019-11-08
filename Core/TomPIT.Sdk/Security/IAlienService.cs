using System;

namespace TomPIT.Security
{
	public interface IAlienService
	{
		IAlien Select(Guid token);
		IAlien Select(string email);
		IAlien SelectByMobile(string mobile);
		IAlien SelectByPhone(string phone);

		Guid Insert(string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone);
		void Update(Guid token, string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone);
		void Delete(Guid token);
	}
}
