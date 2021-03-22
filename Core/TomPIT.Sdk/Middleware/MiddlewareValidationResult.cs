using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TomPIT.Middleware
{
	public class MiddlewareValidationResult : ValidationResult
	{
		public MiddlewareValidationResult(object instance, string errorMessage) : base(errorMessage)
		{
			Instance = instance;
		}

		public MiddlewareValidationResult(object instance, string errorMessage, IEnumerable<string> memberNames) : base(errorMessage, memberNames)
		{
			Instance = instance;
		}

		public object Instance { get; }
	}
}
