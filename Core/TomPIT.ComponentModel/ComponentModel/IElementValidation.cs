using System.Collections.Generic;
using TomPIT.Services;

namespace TomPIT.ComponentModel
{
	public interface IElementValidation
	{
		bool Validate(IExecutionContext context);
		bool IsValid { get; }
		List<IValidationMessage> ValidationMessages();
	}
}
