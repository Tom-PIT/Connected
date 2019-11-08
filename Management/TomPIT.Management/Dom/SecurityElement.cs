using TomPIT.Ide.Dom;
using TomPIT.Ide.Environment;

namespace TomPIT.Management.Dom
{
	public class SecurityElement : DomElement
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
			Items.Add(new UsersElement(this));
			Items.Add(new RolesElement(this));
			Items.Add(new MembershipElement(this));
			Items.Add(new UrlSecurityElement(this));
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, UsersElement.FolderId, true) == 0)
				Items.Add(new UsersElement(this));
			else if (string.Compare(id, RolesElement.FolderId, true) == 0)
				Items.Add(new RolesElement(this));
			else if (string.Compare(id, MembershipElement.FolderId, true) == 0)
				Items.Add(new MembershipElement(this));
			else if (string.Compare(id, UrlSecurityElement.FolderId, true) == 0)
				Items.Add(new UrlSecurityElement(this));
		}
	}
}
