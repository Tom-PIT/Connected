using System.Collections.Generic;
using TomPIT.Routing;

namespace TomPIT.Models
{
	public interface IModelNavigation
	{
		List<IRoute> Breadcrumbs { get; }
		List<IRoute> Links { get; }
	}
}
