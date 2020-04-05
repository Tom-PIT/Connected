using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using TomPIT.Models;
using TomPIT.Serialization;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT
{
	public class IoCHelper : HelperBase
	{
		public IoCHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public async Task<IHtmlContent> Render([CIP(CIP.IoCOperationsProvider)]string operation)
		{
			Html.ViewBag._IoCViewOperation = operation;

			return await Html.PartialAsync("~/Views/IoCView.cshtml", Html.ViewData.Model);
		}

		public async Task<IHtmlContent> Render([CIP(CIP.IoCOperationsProvider)]string operation, object arguments)
		{
			Html.ViewBag._IoCViewOperation = operation;

			var a = arguments == null ? null : Serializer.Deserialize<JObject>(arguments);

			if (a != null && Html.ViewData.Model is IRuntimeModel rtModel)
				rtModel.MergeArguments(a);

			return await Html.PartialAsync("~/Views/IoCView.cshtml", Html.ViewData.Model);
		}
	}
}