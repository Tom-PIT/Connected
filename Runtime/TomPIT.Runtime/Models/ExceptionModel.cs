using Microsoft.AspNetCore.Http;
using System;

namespace TomPIT.Models
{
	internal class ExceptionModel : ShellModel, IUIModel
	{
		public ExceptionModel(HttpContext context, Exception ex)
		{
			Initialize(context.Request, null, null, null, null);

			Title = SR.ViewErrorTitle;
		}
	}
}
