using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Dom.ComponentModel;
using TomPIT.Management.Designers;
using TomPIT.Management.Security;
using TomPIT.Security;

namespace TomPIT.Management.Dom
{
	public class UserElement : TransactionElement
	{
		private IDomDesigner _designer = null;

		public UserElement(IDomElement parent, IUser user) : base(parent)
		{
			User = user;

			Id = user.Token.ToString();
			Title = User.DisplayName();
		}

		public override bool HasChildren => false;
		private IUser User { get; }

		public override object Component => User;

		public override bool Commit(object component, string property, string attribute)
		{
			Environment.Context.Tenant.GetService<IUserManagementService>().Update(User.Token, User.LoginName, User.Email, User.Status, User.FirstName, User.LastName, User.Description, User.Pin,
				User.Language, User.TimeZone, User.NotificationEnabled, User.Mobile, User.Phone, User.SecurityCode);

			return true;
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new UserDesigner(this);

				return _designer;
			}
		}
	}
}
