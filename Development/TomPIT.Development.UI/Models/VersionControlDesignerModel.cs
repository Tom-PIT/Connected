using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Ide.VersionControl;

namespace TomPIT.Development.Models
{
	public class VersionControlDesignerModel : DevelopmentModel
	{
		private List<IRepository> _repositories = null;
		public VersionControlDesignerModel(JObject arguments)
		{
			Arguments = arguments;
		}

		public JObject Arguments { get; }

		public List<IRepository> Repositories
		{
			get
			{
				if (_repositories == null)
					_repositories = Tenant.GetService<IVersionControlService>().QueryRepositories();

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

			Tenant.GetService<IVersionControlService>().UpdateRepository(existingName, name, url, userName, password);
		}

		private void InsertBinding()
		{
			var name = Arguments.Required<string>("name");
			var url = Arguments.Required<string>("url");
			var userName = Arguments.Required<string>("userName");
			var password = Arguments.Required<string>("password");

			Tenant.GetService<IVersionControlService>().InsertRepository(name, url, userName, password);
		}

		public IRepository SelectRepository(string name)
		{
			return Tenant.GetService<IVersionControlService>().SelectRepository(name);
		}
	}
}
