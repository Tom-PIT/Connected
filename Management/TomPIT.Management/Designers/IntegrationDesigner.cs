using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.ComponentModel;
using TomPIT.Designers;
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
                    _microServices = Connection.GetService<IMicroServiceService>().Query().Where(f => f.CommitStatus == CommitStatus.Invalidated
                    || f.CommitStatus == CommitStatus.Publishing
                    || f.CommitStatus == CommitStatus.PublishError).OrderBy(f => f.Name).ToList();

                return _microServices;
            }
        }

        public bool IsLogged { get { return Connection.GetService<IDeploymentService>().IsLogged; } }

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
                var microService = Connection.GetService<IMicroServiceService>().Select(Types.Convert<Guid>(i.Value<string>()));

                if (microService != null)
                    Connection.GetService<IMicroServiceManagementService>().Update(microService.Token, microService.Name, microService.Status, microService.Template,
                        microService.ResourceGroup, microService.Package, microService.UpdateStatus, CommitStatus.Publishing);
            }

            return Result.SectionResult(ViewModel, Annotations.EnvironmentSection.Designer);
        }
    }
}
