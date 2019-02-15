using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.ActionResults;
using TomPIT.Deployment;
using TomPIT.Management.Deployment;
using TomPIT.Management.Dom;

namespace TomPIT.Management.Designers
{
	public class DeploymentDesigner : DeploymentDesignerBase<MarketplaceElement>
	{
		private List<IPublishedPackage> _publicPackages = null;

		public DeploymentDesigner(MarketplaceElement element) : base(element)
		{
		}

		protected override string MainView => "~/Views/Ide/Designers/Deployment.cshtml";
		public override string View => MainView;

		public List<IPublishedPackage> PublicPackages
		{
			get
			{
				if (_publicPackages == null)
					_publicPackages = Connection.GetService<IDeploymentService>().QueryPublicPackages();

				return _publicPackages;
			}
		}

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "install", true) == 0)
				return Install(data);

			return base.OnAction(data, action);
		}

		private IDesignerActionResult Install(JObject data)
		{
			PackageInfo = Connection.GetService<IDeploymentService>().SelectPublishedPackage(data.Required<Guid>("package"));

			return Result.ViewResult(ViewModel, "~/Views/Ide/Designers/DeploymentPackage.cshtml");
			//var packages = data.Required<JArray>("packages");
			//var items = new List<IInstallState>();

			//foreach (JValue i in packages)
			//{
			//	items.Add(new InstallState
			//	{
			//		Package = Types.Convert<Guid>(i.Value<string>())
			//	});
			//}

			//Connection.GetService<IDeploymentService>().InsertInstallers(items);

			//return Result.EmptyResult(ViewModel);
		}

		public IPublishedPackage PackageInfo { get; private set; }

		public IPackageConfiguration SelectConfiguration(Guid package)
		{
			return Connection.GetService<IDeploymentService>().SelectPackageConfiguration(package);
		}
	}
}
