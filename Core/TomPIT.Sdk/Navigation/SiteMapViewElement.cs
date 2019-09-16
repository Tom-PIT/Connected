using System;
using TomPIT.ComponentModel;
using TomPIT.Security;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Navigation
{
	public abstract class SiteMapViewElement : SiteMapElement, ISiteMapAuthorizationElement
	{
		[CAP(CAP.NavigationViewUrlProvider)]

		public string View { get; set; }

		public bool Authorize(Guid user)
		{
			if (string.IsNullOrWhiteSpace(View))
				return false;

			var cd = ComponentDescriptor.View(Context, View);

			if (cd.Configuration == null)
				return false;

			var args = new AuthorizationArgs(user, Claims.AccessUrl, cd.Configuration.Url);

			args.Schema.Empty = EmptyBehavior.Deny;
			args.Schema.Level = AuthorizationLevel.Pessimistic;

			var ar = Context.Tenant.GetService<IAuthorizationService>().Authorize(Context, args);

			return ar.Success;
		}
	}
}
