using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.ActionResults;
using TomPIT.Deployment;
using TomPIT.Environment;
using TomPIT.Management.Deployment;
using TomPIT.Management.Dom;

namespace TomPIT.Management.Designers
{
	public class DeploymentDesigner : DeploymentDesignerBase<MarketplaceElement>
	{
		private List<IPublishedPackage> _publicPackages = null;
		private List<IResourceGroup> _resourceGroups = null;

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
			else if (string.Compare(action, "installConfirm", true) == 0)
				return InstallConfirm(data);
			else if (string.Compare(action, "setOption", true) == 0)
				return SetOption(data);

			return base.OnAction(data, action);
		}

		private IDesignerActionResult SetOption(JObject data)
		{
			var package = data.Required<Guid>("package");
			var option = data.Required<string>("option");

			var config = Connection.GetService<IDeploymentService>().SelectInstallerConfiguration(package);

			if (string.Compare(option, "runtimeConfiguration", true) == 0)
				((PackageConfiguration)config).RuntimeConfiguration = data.Required<bool>("value");
			else if (string.Compare(option, "resourceGroup", true) == 0)
				((PackageConfiguration)config).ResourceGroup = data.Required<Guid>("value");

			Connection.GetService<IDeploymentService>().UpdateInstallerConfiguration(package, config);

			return Result.EmptyResult(ViewModel);
		}

		private IDesignerActionResult InstallConfirm(JObject data)
		{
			var packages = data.Required<JArray>("packages");
			var items = new List<IInstallState>();

			foreach (JValue i in packages)
			{
				items.Add(new InstallState
				{
					Package = Types.Convert<Guid>(i.Value<string>())
				});
			}

			Connection.GetService<IDeploymentService>().InsertInstallers(items);

			return Result.EmptyResult(ViewModel);
		}

		private IDesignerActionResult Install(JObject data)
		{
			PackageInfo = Connection.GetService<IDeploymentService>().SelectPublishedPackage(data.Required<Guid>("package"));

			return Result.ViewResult(ViewModel, "~/Views/Ide/Designers/DeploymentPackage.cshtml");
		}

		public IPublishedPackage PackageInfo { get; private set; }

		public List<IResourceGroup> ResourceGroups
		{
			get
			{
				if (_resourceGroups == null)
					_resourceGroups = Connection.GetService<IResourceGroupService>().Query();

				return _resourceGroups;
			}
		}
	}
}
