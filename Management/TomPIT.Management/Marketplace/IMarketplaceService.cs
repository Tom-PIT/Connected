using Newtonsoft.Json.Linq;
using System;

namespace TomPIT.Marketplace
{
	public interface IMarketplaceService
	{
		bool IsLogged { get; }
		IPublisher Publisher { get; }
		void LogIn(string userName, string password, bool permanent);
		Guid SignUp(string company, string firstName, string lastName, string password, string email, int country, string phone, string website);

		bool IsConfirmed(Guid publisherKey);

		JObject QueryCountries();
	}
}
