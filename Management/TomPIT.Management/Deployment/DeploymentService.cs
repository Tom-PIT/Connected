using Newtonsoft.Json.Linq;
using TomPIT.Connectivity;

namespace TomPIT.Deployment
{
	internal class DeploymentService : IDeploymentService
	{
		private const string MarketplaceUrl = "http://localhost/marketplace/rest";

		public DeploymentService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void LogIn(string userName, string password, bool permanent)
		{
			var url = string.Format("{0}/Identity/IIdentity/Authenticate", MarketplaceUrl);
			var body = new JObject
			{
				{"user", userName },
				{"password", password }
			};

			var ar = Connection.Post<AuthenticationResult>(url, body);

			if (ar.Success)
			{
				IsLogged = true;
			}
		}

		public bool IsLogged { get; private set; }
		public bool IsPublisher { get; private set; }
	}
}
