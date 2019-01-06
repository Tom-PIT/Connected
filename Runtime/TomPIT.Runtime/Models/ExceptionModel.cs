using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.Models;
using TomPIT.Runtime;

namespace TomPIT.Exceptions
{
	internal class ExceptionModel : ApplicationContext, IUIModel
	{
		public IModelNavigation Navigation => null;

		public ExceptionModel(HttpContext context, Exception ex)
		{
			Initialize(context.Request, null, null, null, null);

			Title = SR.ViewErrorTitle;
		}

		public string Title { get; set; }

		public void Initialize(Controller controller)
		{

		}

		public IEnumerable<ValidationResult> Validate()
		{
			return null;
		}
	}
}
