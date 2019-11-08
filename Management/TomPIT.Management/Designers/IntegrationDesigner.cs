using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Management.ComponentModel;
using TomPIT.Management.Deployment;
using TomPIT.Management.Dom;

namespace TomPIT.Management.Designers
{
	public class IntegrationDesigner : DomDesigner<IntegrationElement>
	{
		private List<IMicroService> _microServices = null;

		public IntegrationDesigner(IntegrationElement element) : base(element)
		{
		}

		public override string View => "~/Views/Ide/Designers/Integration.cshtml";
		public override object ViewModel => this;
		public List<IMicroService> MicroServices
		{
			get
			{
				if (_microServices == null)
					_microServices = Environment.Context.Tenant.GetService<IMicroServiceService>().Query().Where(f => f.CommitStatus == CommitStatus.Invalidated
					|| f.CommitStatus == CommitStatus.Publishing
					|| f.CommitStatus == CommitStatus.PublishError).OrderBy(f => f.Name).ToList();

				return _microServices;
			}
		}

		public bool IsLogged { get { return Environment.Context.Tenant.GetService<IDeploymentService>().IsLogged; } }

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "publish", true) == 0)
				return Publish(data);

			return base.OnAction(data, action);
		}

		private IDesignerActionResult Publish(JObject data)
		{
			var packages = data.Required<JArray>("packages");

			foreach (JValue i in packages)
			{
				var microService = Environment.Context.Tenant.GetService<IMicroServiceService>().Select(Types.Convert<Guid>(i.Value<string>()));

				if (microService != null)
					Environment.Context.Tenant.GetService<IMicroServiceManagementService>().Update(microService.Token, microService.Name, microService.Status, microService.Template,
						 microService.ResourceGroup, microService.Package, microService.Plan, microService.UpdateStatus, CommitStatus.Publishing);
			}

			return Result.SectionResult(ViewModel, EnvironmentSection.Designer);
		}
	}
}
