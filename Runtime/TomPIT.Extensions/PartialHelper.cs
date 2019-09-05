using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
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
			ProcessView(name);

			return await Html.PartialAsync(string.Format("~/Views/Dynamic/Partial/{0}.cshtml", name), Html.ViewData.Model);
		}


		public async Task<IHtmlContent> Render(string name, object arguments)
		{
			var a = arguments == null ? null : Types.Deserialize<JObject>(Types.Serialize(arguments));

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

		private IPartialView ResolveView(string qualifier)
		{
			var tokens = qualifier.Split('/');
			var model = Html.ViewData.Model as IRuntimeModel;
			var ms = model.MicroService;
			var name = qualifier;

			if (tokens.Length > 1)
			{
				ms = model.Connection().GetService<IMicroServiceService>().Select(tokens[0]);

				if (ms == null)
					return null;

				name = tokens[1];
			}

			return model.Connection().GetService<IComponentService>().SelectConfiguration(ms.Token, "Partial", name) as IPartialView;
		}

		private void ProcessView(string name)
		{
			var view = ResolveView(name);

			if (view == null)
				return;

			var args = new ViewInvokeArguments(Html.ViewData, Html.TempData, Html.ViewBag);

			args.Model.Connection().GetService<ICompilerService>().Execute(((IConfiguration)view).MicroService(args.Model.Connection()), view.Invoke, this, args);
		}
	}
}