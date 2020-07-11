using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Design;

namespace TomPIT.Development.Models
{
	public class VersionControlDesignerModel : DevelopmentModel
	{
		private List<IRepositoriesEndpoint> _repositories = null;
		public VersionControlDesignerModel(JObject arguments)
		{
			Arguments = arguments;
		}

		public JObject Arguments { get; }

		public List<IRepositoriesEndpoint> Repositories
		{
			get
			{
				if (_repositories == null)
					_repositories = Tenant.GetService<IDesignService>().Repositories.Query();

				return _repositories;
			}
		}

		public void DesignerAction()
		{
			var action = Arguments.Required<string>("action");

			if (string.Compare(action, "InsertBinding", true) == 0)
				InsertBinding();
			else if (string.Compare(action, "UpdateBinding", true) == 0)
				UpdateBinding();
		}

		private void UpdateBinding()
		{
			var existingName = Arguments.Required<string>("existingName");
			var name = Arguments.Required<string>("name");
			var url = Arguments.Required<string>("url");
			var userName = Arguments.Required<string>("userName");
			var password = Arguments.Required<string>("password");

			Tenant.GetService<IDesignService>().Repositories.Update(existingName, name, url, userName, password);
		}

		private void InsertBinding()
		{
			var name = Arguments.Required<string>("name");
			var url = Arguments.Required<string>("url");
			var userName = Arguments.Required<string>("userName");
			var password = Arguments.Required<string>("password");

			Tenant.GetService<IDesignService>().Repositories.Insert(name, url, userName, password);
		}

		public IRepositoriesEndpoint SelectRepository(string name)
		{
			return Tenant.GetService<IDesignService>().Repositories.Select(name);
		}

		public List<object> QueryBranches()
		{
			var binding = Arguments.Required<string>("binding");
			var repo = Repositories.FirstOrDefault(f => string.Compare(f.Name, binding, true) == 0);
			var url = $"{repo.Url}/VersionControl/IBranches/QueryBranches";
			var args = new JObject
			{
				{"repository", repo.Name }
			};

			var result = Tenant.Post<List<object>>(url, args);

			return null;
		}
	}
}
