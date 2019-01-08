using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Ide;
using TomPIT.Security;

namespace TomPIT.Dom
{
	public class UserElement : TransactionElement
	{
		private IDomDesigner _designer = null;

		public UserElement(IEnvironment environment, IDomElement parent, IUser user) : base(environment, parent)
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
			Connection.GetService<IUserManagementService>().Update(User.Token, User.LoginName, User.Email, User.Status, User.FirstName, User.LastName, User.Description, User.Pin,
				User.Language, User.TimeZone, User.NotificationEnabled, User.Mobile, User.Phone);

			return true;
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new UserDesigner(Environment, this);

				return _designer;
			}
		}
	}
}
