using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.ActionResults;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Security;

namespace TomPIT.Design
{
	public class MembershipDesigner : DomDesigner<MembershipElement>
	{
		private List<IRole> _roles = null;
		private List<IUser> _users = null;
		private List<IUser> _all = null;
		private List<IUser> _membershipUsers = null;
		private List<IMembership> _membership = null;

		public MembershipDesigner(IEnvironment environment, MembershipElement element) : base(environment, element)
		{
			if (Roles != null && Roles.Count > 0)
				SelectedRole = Roles[0].Token;
		}

		public override string View => "~/Views/Ide/Designers/Membership.cshtml";
		public override object ViewModel => this;
		public Guid SelectedRole { get; private set; }

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "add", true) == 0)
				return Add(data);
			else if (string.Compare(action, "remove", true) == 0)
				return Remove(data);
			else if (string.Compare(action, "loadUsers", true) == 0)
				return LoadUsers(data);
			else if (string.Compare(action, "loadMembership", true) == 0)
				return LoadMembership(data);

			return base.OnAction(data, action);
		}

		private IDesignerActionResult LoadMembership(JObject data)
		{
			SelectedRole = data.Required<Guid>("role");

			return Result.ViewResult(this, "~/Views/Ide/Designers/MembershipList.cshtml");
		}

		private IDesignerActionResult LoadUsers(JObject data)
		{
			SelectedRole = data.Required<Guid>("role");

			return Result.ViewResult(this, "~/Views/Ide/Designers/MembershipUsers.cshtml");
		}

		private IDesignerActionResult Add(JObject data)
		{
			SelectedRole = data.Required<Guid>("role");
			var user = data.Required<Guid>("user");

			SysContext.GetService<IMembershipManagementService>().Insert(user, SelectedRole);

			return Result.ViewResult(this, "~/Views/Ide/Designers/MembershipList.cshtml");
		}

		private IDesignerActionResult Remove(JObject data)
		{
			SelectedRole = data.Required<Guid>("role");
			var user = data.Required<Guid>("user");

			SysContext.GetService<IMembershipManagementService>().Delete(user, SelectedRole);

			return Result.ViewResult(this, "~/Views/Ide/Designers/MembershipUsers.cshtml");
		}

		public List<IRole> Roles
		{
			get
			{
				if (_roles == null)
					_roles = SysContext.GetService<IRoleService>().Query().Where(f => f.Behavior == RoleBehavior.Explicit).OrderBy(f => f.Name).ToList();

				return _roles;
			}
		}

		public List<IUser> All
		{
			get
			{
				if (_all == null)
					_all = SysContext.GetService<IUserService>().Query();

				return _all;
			}
		}

		public List<IUser> Users
		{
			get
			{
				if (_users == null)
				{
					_users = new List<IUser>();

					foreach (var i in All)
					{
						if (MembershipList.FirstOrDefault(f => f.User == i.Token) == null)
							_users.Add(i);
					}
				}

				return _users;
			}
		}

		public List<IUser> Membership
		{
			get
			{
				if (_membershipUsers == null)
				{
					_membershipUsers = new List<IUser>();

					foreach (var i in MembershipList)
					{
						var u = All.FirstOrDefault(f => f.Token == i.User);

						if (u == null)
						{
							SysContext.LogWarning(null, "MembershipDesigner", string.Format("Membership user not found (role:'{0}', user:'{1}')", i.User, SelectedRole));

							continue;
						}

						_membershipUsers.Add(u);
					}
				}

				return _membershipUsers;
			}
		}

		private List<IMembership> MembershipList
		{
			get
			{
				if (_membership == null)
					_membership = SysContext.GetService<IMembershipManagementService>().Query(SelectedRole);

				return _membership;
			}
		}
	}
}
