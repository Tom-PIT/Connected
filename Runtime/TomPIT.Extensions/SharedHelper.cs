using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace TomPIT
{
	public class SharedHelper : HelperBase
	{
		public SharedHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public async Task<IHtmlContent> Globalize()
		{
			return await Html.PartialAsync("~/Views/Shared/Globalize.cshtml");
		}
	}
}
