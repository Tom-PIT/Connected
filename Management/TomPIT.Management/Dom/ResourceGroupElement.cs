using System;
using TomPIT.Design.Ide.Dom;
using TomPIT.Environment;
using TomPIT.Ide.Dom.ComponentModel;
using TomPIT.Management.Environment;

namespace TomPIT.Management.Dom
{
    internal class ResourceGroupElement : TransactionElement, IResourceGroupScope
    {
        public ResourceGroupElement(IDomElement parent, IResourceGroup resourceGroup) : base(parent)
        {
            ResourceGroup = resourceGroup;
            Title = ResourceGroup.Name;
            Id = ResourceGroup.Token.ToString();
        }

        public IResourceGroup ResourceGroup { get; }
        private IServerResourceGroup ManagementResourceGroup => ResourceGroup as IServerResourceGroup;
        public override object Component => ResourceGroup;
        public override bool HasChildren { get { return true; } }

        public override void LoadChildren()
        {
            Items.Add(new MicroServicesElement(this));
            Items.Add(new AuthenticationTokensElement(this));
            Items.Add(new SettingsElement(this));
        }

        public override void LoadChildren(string id)
        {
            if (id.Equals(MicroServicesElement.ElementId, StringComparison.OrdinalIgnoreCase))
                Items.Add(new MicroServicesElement(this));
            else if (id.Equals(AuthenticationTokensElement.ElementId, StringComparison.OrdinalIgnoreCase))
                Items.Add(new AuthenticationTokensElement(this));
            else if (id.Equals(SettingsElement.ElementId, StringComparison.OrdinalIgnoreCase))
                Items.Add(new SettingsElement(this));
        }

        public override bool Commit(object component, string property, string attribute)
        {
            Environment.Context.Tenant.GetService<IResourceGroupManagementService>().Update(ManagementResourceGroup.Token, ManagementResourceGroup.Name,
                ManagementResourceGroup.StorageProvider, ManagementResourceGroup.ConnectionString);

            return true;
        }
    }
}
