using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Search;

namespace TomPIT.Models
{
	public class SearchModel : AjaxModel
	{
		private ISearchOptions _options = null;

		public IClientSearchResults Search()
		{
			return Services.Search.Search(Options);
		}

		private ISearchOptions Options
		{
			get
			{
				if (_options == null)
					_options = Types.Deserialize<SearchOptions>(Types.Serialize(Body));

				return _options;
			}
		}
	}
}
