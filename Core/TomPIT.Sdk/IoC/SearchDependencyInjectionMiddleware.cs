using System.Collections.Generic;
using TomPIT.Middleware;
using TomPIT.Search;

namespace TomPIT.IoC
{
	public abstract class SearchDependencyInjectionMiddleware : MiddlewareObject, ISearchDependencyInjectionMiddleware
	{
		private List<string> _properties = null;
		public List<ISearchEntity> Index(List<ISearchEntity> items)
		{
			return OnIndex(items);
		}

		protected virtual List<ISearchEntity> OnIndex(List<ISearchEntity> items)
		{
			return items;
		}

		public ISearchEntity Search(ISearchEntity searchResult)
		{
			return OnSearch(searchResult);
		}

		protected virtual ISearchEntity OnSearch(ISearchEntity searchResult)
		{
			return searchResult;
		}

		public List<string> Properties
		{
			get
			{
				if (_properties == null)
				{
					_properties = OnCreateProperties();

					if (_properties == null)
						_properties = new List<string>();
				}

				return _properties;
			}
		}

		protected virtual List<string> OnCreateProperties()
		{
			return new List<string>();
		}
	}
}
