using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TomPIT.Annotations;
using TomPIT.ComponentModel.Navigation;
using TomPIT.Services;

namespace TomPIT.Navigation
{
	public abstract class SiteMapViewElement : SiteMapElement, ISiteMapContextElement
	{
		[JsonIgnore]
		public IDataModelContext Context { get; set; }

		[CodeAnalysisProvider(CodeAnalysisProviderAttribute.NavigationViewUrlProvider)]

		public string View { get; set; }
	}
}
