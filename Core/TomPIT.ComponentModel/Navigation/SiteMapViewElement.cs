using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Navigation;
using TomPIT.ComponentModel.UI;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT.Navigation
{
	public abstract class SiteMapViewElement : SiteMapElement, ISiteMapContextElement, ISiteMapAuthorizationElement
	{
		[JsonIgnore]
		public IDataModelContext Context { get; set; }

		[CodeAnalysisProvider(CodeAnalysisProviderAttribute.NavigationViewUrlProvider)]

		public string View { get; set; }

		public bool Authorize(Guid user)
		{
			if (string.IsNullOrWhiteSpace(View))
				return false;

			var cd = new ConfigurationDescriptor<IView>(Context, View, "View");

			if (cd.Configuration == null)
				return false;

			var args = new AuthorizationArgs(user, Claims.AccessUrl, cd.Configuration.Url);

			args.Schema.Empty = EmptyBehavior.Deny;
			args.Schema.Level = AuthorizationLevel.Pessimistic;

			var ar = Context.Connection().GetService<IAuthorizationService>().Authorize(Context, args);

			return ar.Success;
		}
	}
}
