using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using TomPIT.ComponentModel;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Serialization;

namespace TomPIT
{
	public class ReportHelper : HelperBase
	{
		public ReportHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public async Task<IHtmlContent> Render(string name)
		{
			return await Render(name, null);
		}

		public async Task<IHtmlContent> Render(string name, object arguments)
		{
			var context = Html.ViewData.Model as IMicroServiceContext;

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

			var queryString = string.Empty;

			if (arguments != null)
				queryString = Convert.ToBase64String(Encoding.UTF8.GetBytes(Serializer.Serialize(arguments)));

			Html.ViewData[$"parameters{microService}/{report}"] = queryString;

			return await Html.PartialAsync($"~/Views/Dynamic/Report/{microService}/{report}.cshtml", Html.ViewData.Model);
		}
	}
}
