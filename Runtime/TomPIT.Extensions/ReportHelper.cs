using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT
{
	public class ReportHelper : HelperBase
	{
		public ReportHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public async Task<IHtmlContent> Render(string name, string queryString = null)
		{
			var context = Html.ViewData.Model as IMiddlewareContext;

			if (context == null)
				throw new RuntimeException(nameof(ReportHelper), SR.ErrExecutionContextExpected);

			var microService = context.MicroService.Name;
			var report = name;

			if (name.Contains('/'))
			{
				var tokens = name.Split('/');

				context.MicroService.ValidateMicroServiceReference(tokens[0]);

				microService = tokens[0];
				report = tokens[1];
			}

			if (!string.IsNullOrWhiteSpace(queryString))
				Html.ViewData[$"parameters{microService}/{report}"] = queryString;

			return await Html.PartialAsync($"~/Views/Dynamic/Report/{microService}/{report}.cshtml", Html.ViewData.Model);
		}
	}
}
