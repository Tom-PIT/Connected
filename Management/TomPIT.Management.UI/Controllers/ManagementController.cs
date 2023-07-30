using Lucene.Net.Analysis;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;

using TomPIT.Deployment;
using TomPIT.Environment;
using TomPIT.Ide.Controllers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Models;
using TomPIT.Management.Deployment;
using TomPIT.Management.Environment;
using TomPIT.Management.Models;
using TomPIT.Sys.Model.Environment;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TomPIT.Management.Controllers
{
	[Authorize(Roles = "Full Control")]
	public class ManagementController : IdeControllerBase
	{
		protected override IdeModelBase CreateModel()
		{
			var r = new HomeModel();

			r.Initialize(this, null);

			if (string.IsNullOrWhiteSpace(Request.ContentType)
				 || Request.ContentType.Contains("application/json"))
				r.RequestBody = FromBody();

			r.Databind();

			return r;
		}

		[HttpPost]
		public IActionResult InstallPackage()
		{
			var model = CreateModel();

			var packageName = model.RequestBody.Required<string>("name");
			var planName = model.RequestBody.Required<string>("plan");

			var deploymentService = Tenant.GetService<IDeploymentService>();

			var plan = deploymentService.QueryMyPlans().FirstOrDefault(e => e.Name == planName);

			var package = deploymentService.QueryPackages(plan.Token).FirstOrDefault(e=> e.Name == packageName)?.Token;

			if (package is null)
				return NotFound();

			var r = new List<IInstallState>();
			var pcg = Tenant.GetService<IDeploymentService>().SelectPublishedPackage(package.Value);
			var dependencyConfiguration = Tenant.GetService<IDeploymentService>().QueryDependencies(pcg.Service, pcg.Plan);
			var config = Tenant.GetService<IDeploymentService>().SelectInstallerConfiguration(package.Value);

			var criteria = new List<Tuple<Guid, Guid>>();

			foreach (var dependency in dependencyConfiguration)
				criteria.Add(new Tuple<Guid, Guid>(dependency.MicroService, dependency.Plan));

			var dependencyPackages = criteria.Count > 0 ? Tenant.GetService<IDeploymentService>().QueryPublishedPackages(criteria) : new List<IPublishedPackage>();

			r.Add(CreateInstallState(pcg.Token, Guid.Empty));

			foreach (var dependency in dependencyConfiguration)
			{
				var targetPackage = dependencyPackages.FirstOrDefault(f => f.Service == dependency.MicroService && f.Plan == dependency.Plan);

				if (targetPackage == null)
					continue;

				var d = config.Dependencies.FirstOrDefault(f => f.Dependency == targetPackage.Token);

				if (d != null && !d.Enabled)
					continue;

				var dependencyPackageConfiguration = Tenant.GetService<IDeploymentService>().SelectInstallerConfiguration(targetPackage.Token);

				r.Add(CreateInstallState(targetPackage.Token, pcg.Token));
			}

			Tenant.GetService<IDeploymentService>().InsertInstallers(r);

			return Ok();
		}

		private IInstallState CreateInstallState(Guid package, Guid parent)
		{
			return new InstallState
			{
				Package = package,
				Parent = parent
			};
		}

		private class InstallState : IInstallState
		{
			public Guid Package { get; set; }

			public Guid Parent { get; set; }

			public InstallStateStatus Status { get; set; }

			public DateTime Modified { get; set; }

			public string Error { get; set; }
		}
	}
}
