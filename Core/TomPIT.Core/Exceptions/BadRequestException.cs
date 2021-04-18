using System;

namespace TomPIT.Exceptions
{
	public class BadRequestException : RuntimeException
	{
		public BadRequestException(string message) : base(message)
		{
		}

		public BadRequestException(string message, Exception inner) : base(message, inner)
		{
		}

		public BadRequestException(string source, string message) : base(source, message)
		{
		}

		public BadRequestException(string source, string message, string stackTrace) : base(source, message, stackTrace)
		{
		}
	}
}
