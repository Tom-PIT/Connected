using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.Actions;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Items;
using TomPIT.Security;

namespace TomPIT.Design
{
	internal class UsersDesigner : CollectionDesigner<UsersElement>
	{
		public UsersDesigner(IEnvironment environment, UsersElement element) : base(environment, element)
		{

		}

		protected override IDesignerActionResult Add(IItemDescriptor d)
		{
			if (string.Compare(d.Id, UsersCollection.User, true) == 0)
				return CreateUser();

			return Result.EmptyResult(this);
		}

		protected override IDesignerActionResult Remove(JObject data)
		{
			var id = data.Argument("id", Guid.Empty);

			if (id == Guid.Empty)
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "id");

			var user = Owner.Existing.FirstOrDefault(f => f.Token == id);

			SysContext.GetService<IUserManagementService>().Delete(user.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		protected override IDesignerActionResult Clear(JObject data)
		{
			foreach (var i in Owner.Existing)
				SysContext.GetService<IUserManagementService>().Delete(i.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult CreateUser()
		{
			var existing = Owner.Existing;
			var name = SysContext.GetService<INamingService>().Create("User", existing.Select(f => f.LoginName), true);

			var id = SysContext.GetService<IUserManagementService>().Insert(name, string.Empty, UserStatus.Inactive, string.Empty, string.Empty, string.Empty, null, string.Empty,
				Guid.Empty, string.Empty, true, string.Empty, string.Empty);

			var user = SysContext.GetService<IUserService>().Select(id.ToString());

			var r = Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);

			r.MessageKind = InformationKind.Success;
			r.Message = string.Format(SR.UserCreateSuccess, name);
			r.Title = SR.DevCreateUser;
			r.ExplorerPath = string.Format("{0}/{1}", DomQuery.Path(Element), user.Token);

			return r;
		}

		protected override bool OnCreateToolbarAction(IDesignerToolbarAction action)
		{
			return action.Id != Undo.ActionId;
		}

		public override bool SupportsReorder
		{
			get { return false; }
		}
	}
}
