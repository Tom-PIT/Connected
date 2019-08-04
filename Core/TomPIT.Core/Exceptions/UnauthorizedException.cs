using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Exceptions
{
	public class UnauthorizedException : RuntimeException
	{
		public UnauthorizedException(string message) : base(message)
		{
		}

		public UnauthorizedException(string message, Exception inner) : base(message, inner)
		{
		}

		public UnauthorizedException(string source, string message) : base(source, message)
		{
		}

		public UnauthorizedException(string source, string message, string stackTrace) : base(source, message, stackTrace)
		{
		}
	}
}
