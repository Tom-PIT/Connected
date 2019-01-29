using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using TomPIT.Services;

namespace TomPIT
{
	public class PartialHelper : HelperBase
	{
		public PartialHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public async Task<IHtmlContent> Render(string name)
		{
			return await Html.PartialAsync(string.Format("~/Views/Dynamic/Partial/{0}.cshtml", name), new ExecutionContext(Html.ViewData.Model as IExecutionContext));
		}
	}
}
