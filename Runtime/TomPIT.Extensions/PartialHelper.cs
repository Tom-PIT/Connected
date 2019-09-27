using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Models;
using TomPIT.Serialization;
using TomPIT.UI;

namespace TomPIT
{
	public class PartialHelper : HelperBase
	{
		public PartialHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public async Task<IHtmlContent> Render(string name)
		{
			ProcessView(name);

			return await Html.PartialAsync(string.Format("~/Views/Dynamic/Partial/{0}.cshtml", name), Html.ViewData.Model);
		}


		public async Task<IHtmlContent> Render(string name, object arguments)
		{
			var a = arguments == null ? null : Serializer.Deserialize<JObject>(arguments);

			if (a != null && Html.ViewData.Model is IRuntimeModel rtModel)
				rtModel.MergeArguments(a);

			ProcessView(name);

			return await Html.PartialAsync(string.Format("~/Views/Dynamic/Partial/{0}.cshtml", name), Html.ViewData.Model);
		}

		public async Task<IHtmlContent> Render(string name, JObject arguments)
		{
			if (arguments != null && Html.ViewData.Model is IRuntimeModel rtModel)
				rtModel.MergeArguments(arguments);

			ProcessView(name);

			return await Html.PartialAsync(string.Format("~/Views/Dynamic/Partial/{0}.cshtml", name), Html.ViewData.Model);
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

		private void ProcessView(string name)
		{
			var view = ResolveView(name);

			if (view == null)
				return;

			var args = new ViewInvokeArguments(Html.ViewData, Html.TempData, Html.ViewBag);

			args.Model.Tenant.GetService<ICompilerService>().Execute(((IConfiguration)view).MicroService(), view.Invoke, this, args);
		}
	}
}