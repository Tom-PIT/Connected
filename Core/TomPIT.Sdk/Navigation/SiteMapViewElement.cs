using System;
using TomPIT.ComponentModel;
using TomPIT.Security;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Navigation
{
	public abstract class SiteMapViewElement : SiteMapElement, ISiteMapAuthorizationElement
	{
		[CIP(CIP.SiteMapViewProvider)]

		public string View { get; set; }

		//public object Parameters { get; set; }
		public string RouteKey { get; set; }
		public bool Authorize(Guid user)
		{
			if (string.IsNullOrWhiteSpace(View))
				return false;

			var cd = ComponentDescriptor.View(Context, View);

			if (cd.Configuration == null)
				return false;

			var args = new AuthorizationArgs(user, Claims.AccessUrl, cd.Configuration.Url, "Url");

			args.Schema.Empty = EmptyBehavior.Deny;
			args.Schema.Level = AuthorizationLevel.Pessimistic;

			var ar = Context.Tenant.GetService<IAuthorizationService>().Authorize(Context, args);

			return ar.Success;
		}
	}
}
