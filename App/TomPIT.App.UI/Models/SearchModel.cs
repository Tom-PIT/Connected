using TomPIT.Search;
using TomPIT.Serialization;

namespace TomPIT.App.Models
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
					_options = Serializer.Deserialize<SearchOptions>(Serializer.Serialize(Body));

				return _options;
			}
		}
	}
}
