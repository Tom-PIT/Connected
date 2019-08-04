using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Exceptions
{
	public class NotFoundException : RuntimeException
	{
		public NotFoundException(string message) : base(message)
		{
		}

		public NotFoundException(string message, Exception inner) : base(message, inner)
		{
		}

		public NotFoundException(string source, string message) : base(source, message)
		{
		}

		public NotFoundException(string source, string message, string stackTrace) : base(source, message, stackTrace)
		{
		}
	}
}
