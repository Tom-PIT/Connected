using TomPIT.Security;

namespace TomPIT.Dom
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
			Connection.GetService<IRoleManagementService>().Update(Role.Token, Role.Name);

			return true;
		}
	}
}
