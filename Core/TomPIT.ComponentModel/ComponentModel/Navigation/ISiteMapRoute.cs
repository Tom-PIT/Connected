using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;

namespace TomPIT.Navigation
{
	public interface ISiteMapRoute : ISiteMapElement
	{
		[CodeAnalysisProvider(CodeAnalysisProviderAttribute.NavigationUrlProvider)]
		string Template { get; }

		string RouteKey { get; }

		ConnectedList<ISiteMapRoute, ISiteMapRoute> Routes { get; }
	}
}
