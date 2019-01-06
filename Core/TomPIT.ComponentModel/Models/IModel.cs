using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.Services;

namespace TomPIT.Models
{
	public interface IModel : IExecutionContext
	{
		IEnumerable<ValidationResult> Validate();
		void Initialize(Controller controller);
	}
}
