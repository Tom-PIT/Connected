using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.Ide;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Designers.Toolbar;
using TomPIT.Management.Dom;
using TomPIT.Management.Items;
using TomPIT.Management.Security;
using TomPIT.Security;

namespace TomPIT.Management.Designers
{
	internal class AuthenticationTokensDesigner : CollectionDesigner<AuthenticationTokensElement>
	{
		public AuthenticationTokensDesigner(AuthenticationTokensElement element) : base(element)
		{

		}

		protected override IDesignerActionResult Add(IItemDescriptor d)
		{
			if (string.Compare(d.Id, AuthenticationTokensCollection.Token, true) == 0)
				return CreateToken();

			return Result.EmptyResult(this);
		}

		protected override IDesignerActionResult Remove(JObject data)
		{
			var id = data.Optional("id", Guid.Empty);

			if (id == Guid.Empty)
				throw IdeException.ExpectedParameter(this, IdeEvents.DesignerAction, "id");

			var token = Owner.Existing.FirstOrDefault(f => f.Token == id);

			Environment.Context.Tenant.GetService<IAuthenticationTokenManagementService>().Delete(token.Token);

			return Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);
		}

		private IDesignerActionResult CreateToken()
		{
			var existing = Owner.Existing;
			var key = Guid.NewGuid().ToString();
			var rg = DomQuery.Closest<IResourceGroupScope>(Owner).ResourceGroup.Token;
			/*
			 * performance enhancement:
			 * find a way to pull a specific user from the system 
			 * without querying the entire set
			 */
			var users = Environment.Context.Tenant.GetService<IUserService>().Query();
			var id = Environment.Context.Tenant.GetService<IAuthenticationTokenManagementService>().Insert(rg, users[0].Token, "AuthenticationToken", null, key, AuthenticationTokenClaim.None, AuthenticationTokenStatus.Disabled, DateTime.MinValue,
				DateTime.MinValue, TimeSpan.Zero, TimeSpan.Zero, string.Empty);

			var token = Environment.Context.Tenant.GetService<IAuthenticationTokenManagementService>().Select(id);

			var r = Result.SectionResult(this, EnvironmentSection.Designer | EnvironmentSection.Explorer);

			r.MessageKind = InformationKind.Success;
			r.Message = string.Format("Authentication token '{0}' successfully created.", key);
			r.Title = "Create authentication token";
			r.ExplorerPath = string.Format("{0}/{1}", DomQuery.Path(Element), token.Token);

			return r;
		}

		protected override bool OnCreateToolbarAction(IDesignerToolbarAction action)
		{
			return action.Id != Undo.ActionId && action.Id != Ide.Designers.Toolbar.Clear.ActionId;
		}

		public override bool SupportsReorder
		{
			get { return false; }
		}
	}
}
