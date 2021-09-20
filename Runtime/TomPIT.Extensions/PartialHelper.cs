using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Middleware;
using TomPIT.Models;
using TomPIT.Serialization;
using TomPIT.UI;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT
{
	public class PartialHelper : HelperBase
	{
		public PartialHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public async Task<IHtmlContent> Render([CIP(CIP.PartialProvider)]string name)
		{
			return await Html.PartialAsync(string.Format("~/Views/Dynamic/Partial/{0}.cshtml", name), Html.ViewData.Model as IMiddlewareContext);
		}


		public async Task<IHtmlContent> Render([CIP(CIP.PartialProvider)]string name, object arguments)
		{
			return await Html.PartialAsync(string.Format("~/Views/Dynamic/Partial/{0}.cshtml", name), CreateModel(arguments));
		}

		public async Task<IHtmlContent> Render([CIP(CIP.PartialProvider)]string name, JObject arguments)
		{
			return await Html.PartialAsync(string.Format("~/Views/Dynamic/Partial/{0}.cshtml", name), CreateModel(arguments));
		}

		private IRuntimeModel CreateModel(object arguments)
		{
			var a = arguments is null ? null : Serializer.Deserialize<JObject>(arguments);
			var runtimeModel = Html.ViewData.Model as IRuntimeModel;
			var partialModel = MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().CreateModel(runtimeModel);

			if (a is not null)
				partialModel.MergeArguments(a);

			return partialModel;
		}

		private IPartialViewConfiguration ResolveView(string qualifier)
		{
			var tokens = qualifier.Split('/');
			var model = Html.ViewData.Model as IRuntimeModel;
			var ms = model.MicroService;
			var name = qualifier;

			if (tokens.Length > 1)
			{
				ms = model.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

				if (ms == null)
					return null;

				name = tokens[1];
			}

			return model.Tenant.GetService<IComponentService>().SelectConfiguration(ms.Token, "Partial", name) as IPartialViewConfiguration;
		}
	}
}