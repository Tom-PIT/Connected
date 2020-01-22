using System.Collections.Generic;
using TomPIT.Navigation;

namespace TomPIT.Models
{
	public class ModelNavigation : IModelNavigation
	{
		private List<IRoute> _breadcrumbs = null;
		private List<IRoute> _links = null;
		private List<IRoute> _menu = null;

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

		public List<IRoute> Menu
		{
			get
			{
				if (_menu == null)
					_menu = new List<IRoute>();

				return _menu;
			}
		}
	}
}
