using System.Collections.Generic;
using TomPIT.Navigation;

namespace TomPIT.Models
{
	public interface IModelNavigation
	{
		List<IRoute> Breadcrumbs { get; }
		List<IRoute> Links { get; }
	}
}
