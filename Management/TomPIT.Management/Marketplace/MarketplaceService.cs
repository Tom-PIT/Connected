using Newtonsoft.Json.Linq;
using System;
using TomPIT.Connectivity;

namespace TomPIT.Marketplace
{
	internal class MarketplaceService : IMarketplaceService
	{
		private const string MarketplaceUrl = "http://localhost/marketplace/rest";

		public MarketplaceService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void LogIn(string userName, string password, bool permanent)
		{
			var url = string.Format("{0}/Marketplace/IIdentity/Authenticate", MarketplaceUrl);
			var body = new JObject
			{
				{"user", userName },
				{"password", password }
			};

			var ar = Connection.Post<AuthenticationResult>(url, body);

			if (ar.Success)
			{
				IsLogged = true;
				AuthenticationToken = ar.Token;

				LoadPublisherProfile();
			}
		}

		private void LoadPublisherProfile()
		{
			var url = string.Format("{0}/Marketplace/IPublishers/SelectByAuthenticationToken", MarketplaceUrl);
			var body = new JObject
			{
				{"token", AuthenticationToken }
			};

			Publisher = Connection.Post<Publisher>(url, body);
		}

		private void LoadPublisherProfile(Guid key)
		{
			var url = string.Format("{0}/Marketplace/IPublishers/SelectByKey", MarketplaceUrl);
			var body = new JObject
			{
				{"publisherKey", key }
			};

			Publisher = Connection.Post<Publisher>(url, body);
		}

		public Guid SignUp(string company, string firstName, string lastName, string password, string email, int country, string phone, string website)
		{
			var url = string.Format("{0}/Marketplace/IPublishers/Insert", MarketplaceUrl);
			var body = new JObject
			{
				{"company", company },
				{"firstName", firstName },
				{"lastName", lastName },
				{"password", password },
				{"email", email },
				{"country", country },
				{"phone", phone },
				{"website", website }
			};

			return Connection.Post<Guid>(url, body);
		}

		public JObject QueryCountries()
		{
			var url = string.Format("{0}/Marketplace/ICountries/Query", MarketplaceUrl);

			return Connection.Get<JObject>(url);
		}

		public bool IsLogged { get; private set; }
		public IPublisher Publisher { get; private set; }

		private string AuthenticationToken { get; set; }
	}
}
