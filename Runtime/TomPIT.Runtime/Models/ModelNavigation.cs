using System.Collections.Generic;
using TomPIT.Routing;

namespace TomPIT.Models
{
	public class ModelNavigation : IModelNavigation
	{
		private List<IRoute> _breadcrumbs = null;
		private List<IRoute> _links = null;

		public List<IRoute> Breadcrumbs
		{
			get
			{
				if (_breadcrumbs == null)
					_breadcrumbs = new List<IRoute>();

				return _breadcrumbs;
			}
		}

		public List<IRoute> Links
		{
			get
			{
				if (_links == null)
					_links = new List<IRoute>();

				return _links;
			}
		}
	}
}
