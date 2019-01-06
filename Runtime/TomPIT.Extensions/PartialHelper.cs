using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TomPIT
{
	public class PartialHelper : HelperBase
	{
		public PartialHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public IHtmlContent Render(string name)
		{
			return Html.Partial(string.Format("~/Views/Dynamic/Partial/{0}.cshtml", name));
		}
	}
}
