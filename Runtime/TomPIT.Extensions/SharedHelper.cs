using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TomPIT
{
	public class SharedHelper : HelperBase
	{
		public SharedHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public IHtmlContent Globalize()
		{
			return Html.Partial("~/Views/Shared/Globalize.cshtml");
		}
	}
}
