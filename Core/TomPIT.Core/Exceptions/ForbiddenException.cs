using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Exceptions
{
	public class ForbiddenException : RuntimeException
	{
		public ForbiddenException(string message) : base(message)
		{
		}

		public ForbiddenException(string message, Exception inner) : base(message, inner)
		{
		}

		public ForbiddenException(string source, string message) : base(source, message)
		{
		}

		public ForbiddenException(string source, string message, string stackTrace) : base(source, message, stackTrace)
		{
		}
	}
}
