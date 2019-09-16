using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TomPIT.ComponentModel;
using TomPIT.Middleware;

namespace TomPIT.Models
{
	public interface IModel : IMiddlewareContext
	{
		IEnumerable<ValidationResult> Validate();
		void Initialize(Controller controller, IMicroService microService);
	}
}
