using System;
using TomPIT.Environment;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	internal class ResourceGroupElement : TransactionElement, IResourceGroupScope
	{
		public ResourceGroupElement(IEnvironment environment, IDomElement parent, IResourceGroup resourceGroup) : base(environment, parent)
		{
			ResourceGroup = resourceGroup;
			Title = ResourceGroup.Name;
			Id = ResourceGroup.Token.AsString();
		}

		public IResourceGroup ResourceGroup { get; }
		private ManagementResourceGroup ManagementResourceGroup => ResourceGroup as ManagementResourceGroup;
		public override object Component => ResourceGroup;
		public override bool HasChildren { get { return true; } }

		public override void LoadChildren()
		{
			Items.Add(new MicroServicesElement(Environment, this));
		}

		public override void LoadChildren(string id)
		{
			if (id.Equals(MicroServicesElement.ElementId, StringComparison.OrdinalIgnoreCase))
				Items.Add(new MicroServicesElement(Environment, this));
		}

		public override bool Commit(object component, string property, string attribute)
		{
			Connection.GetService<IResourceGroupManagementService>().Update(ManagementResourceGroup.Token, ManagementResourceGroup.Name,
				ManagementResourceGroup.StorageProvider, ManagementResourceGroup.ConnectionString);

			return true;
		}
	}
}
