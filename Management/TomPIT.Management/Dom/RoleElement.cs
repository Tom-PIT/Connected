using TomPIT.Ide.Dom;
using TomPIT.Ide.Dom.ComponentModel;
using TomPIT.Management.Security;
using TomPIT.Security;

namespace TomPIT.Management.Dom
{
	public class RoleElement : TransactionElement
	{
		public RoleElement(IDomElement parent, IRole role) : base(parent)
		{
			Role = role;

			Id = role.Token.ToString();
			Title = Role.Name;
		}

		public override bool HasChildren => false;
		private IRole Role { get; }

		public override object Component => Role;

		public override bool Commit(object component, string property, string attribute)
		{
			Environment.Context.Tenant.GetService<IRoleManagementService>().Update(Role.Token, Role.Name);

			return true;
		}
	}
}
