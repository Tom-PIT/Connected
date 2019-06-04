using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using TomPIT.Models;

namespace TomPIT
{
	public class PartialHelper : HelperBase
	{
		public PartialHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public async Task<IHtmlContent> Render(string name)
		{
			return await Html.PartialAsync(string.Format("~/Views/Dynamic/Partial/{0}.cshtml", name), Html.ViewData.Model);
		}

		public async Task<IHtmlContent> Render(string name, JObject arguments)
		{
			if (arguments != null && Html.ViewData.Model is IRuntimeModel rtModel)
					rtModel.MergeArguments(arguments);

			return await Html.PartialAsync(string.Format("~/Views/Dynamic/Partial/{0}.cshtml", name), Html.ViewData.Model);
		}
	}
}
