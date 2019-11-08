using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using TomPIT.Annotations.Search;
using TomPIT.Reflection;
using TomPIT.Search;

namespace TomPIT
{
	public class SearchResultsHelper : HelperBase
	{
		public SearchResultsHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public async Task<IHtmlContent> Render(IClientSearchResult result)
		{
			var partial = "~/Views/Shared/DefaultSearchResult.cshtml";

			if (result.Entity != null)
			{
				var att = result.Entity.GetType().FindAttribute<SearchRendererAttribute>();

				if (att != null)
					partial = $"~/Views/Dynamic/Partial/{att.Partial}.cshtml";
			}

			Html.ViewBag.SearchResult = result;

			return await Html.PartialAsync(partial, Html.ViewData.Model);
		}
	}
}
