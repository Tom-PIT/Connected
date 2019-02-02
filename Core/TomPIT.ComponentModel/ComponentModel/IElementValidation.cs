using System.Collections.Generic;
using TomPIT.Services;

namespace TomPIT.ComponentModel
{
	public interface IElementValidation
	{
		bool IsValid(IExecutionContext context);
		List<string> ValidationErrors();
	}
}
