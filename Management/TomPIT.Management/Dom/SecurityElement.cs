using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class SecurityElement : Element
	{
		public const string FolderId = "Security";

		public SecurityElement(IEnvironment environment) : base(environment, null)
		{
			Id = FolderId;
			Glyph = "fal fa-shield-check";
			Title = SR.DomSecurity;

			((Behavior)Behavior).AutoExpand = true;
		}

		public override bool HasChildren { get { return true; } }

		public override void LoadChildren()
		{
			Items.Add(new UsersElement(Environment, this));
			Items.Add(new RolesElement(Environment, this));
			Items.Add(new MembershipElement(Environment, this));
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, UsersElement.FolderId, true) == 0)
				Items.Add(new UsersElement(Environment, this));
			else if (string.Compare(id, RolesElement.FolderId, true) == 0)
				Items.Add(new RolesElement(Environment, this));
			else if (string.Compare(id, MembershipElement.FolderId, true) == 0)
				Items.Add(new MembershipElement(Environment, this));
		}
	}
}
