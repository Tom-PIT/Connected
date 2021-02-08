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
		public string RouteKey { get; set; }
		public bool Authorize(Guid user)
		{
			if (string.IsNullOrWhiteSpace(View))
				return false;

			var cd = ComponentDescriptor.View(Context, View);

			if (cd.Configuration == null)
				return false;

			if (!cd.Configuration.AuthorizationEnabled)
				return true;

			return SecurityExtensions.AuthorizeUrl(Context, cd.Configuration.Url, user, false);
		}
	}
}
