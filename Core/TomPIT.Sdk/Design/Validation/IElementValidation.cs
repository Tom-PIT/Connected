using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Design.Validation
{
	public interface IElementValidation
	{
		bool Validate(IMiddlewareContext context);
		bool IsValid { get; }
		List<IValidationMessage> ValidationMessages();
	}
}
