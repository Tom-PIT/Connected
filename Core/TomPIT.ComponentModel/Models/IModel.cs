using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TomPIT.Runtime;

namespace TomPIT.Models
{
	public interface IModel : IApplicationContext
	{
		IEnumerable<ValidationResult> Validate();
		void Initialize(Controller controller);
	}
}
