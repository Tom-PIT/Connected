using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using TomPIT.Services;

namespace TomPIT
{
	public class ReportHelper : HelperBase
	{
		public ReportHelper(IHtmlHelper helper) : base(helper)
		{
		}

		public async Task<IHtmlContent> Render(string name)
		{
			var context = Html.ViewData.Model as IExecutionContext;

			if (context == null)
				throw new RuntimeException(nameof(ReportHelper), SR.ErrExecutionContextExpected);

			var microService = context.MicroService.Name;
			var report = name;

			if (name.Contains('/'))
			{
				var tokens = name.Split('/');

				context.MicroService.ValidateMicroServiceReference(context.Connection(), tokens[0]);

				microService = tokens[0];
				report = tokens[1];
			}

			return await Html.PartialAsync($"~/Views/Dynamic/Report/{microService}/{report}.cshtml", Html.ViewData.Model);
		}
	}
}
