using System;

namespace TomPIT.Exceptions
{
	public class ServerException : TomPITException
	{
		public ServerException()
		{
		}

		public ServerException(string message) : base(message)
		{
		}

		public ServerException(string message, params string[] args) : base(message, args)
		{
		}

		public ServerException(string message, Exception inner) : base(message, inner)
		{
		}

		public static ServerException ExpectedArgument(string argument)
		{
			return new ServerException(string.Format("{0} ({1}).", SR.ErrServerExpectedArgument, argument));
		}

		public static ServerException InvalidArgumentType(string argument, Type expected)
		{
			return new ServerException(string.Format("{0} ({1}, {2}).", SR.ErrServerInvalidArgumentType, argument, expected.Name));
		}

		public static ServerException MicroServiceNotFound()
		{
			return new ServerException(SR.ErrMicroServiceNotFound);
		}

		public static ServerException UserNotFound()
		{
			return new ServerException(SR.ErrUserNotFound);
		}
	}
}
