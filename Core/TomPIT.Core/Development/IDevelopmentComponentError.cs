using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Development
{
	public interface IDevelopmentComponentError
	{
		Guid Element { get; }
		DevelopmentSeverity Severity { get; }
		string Message { get; }
	}
}
