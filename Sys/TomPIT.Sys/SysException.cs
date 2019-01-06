using System;

namespace TomPIT.Sys
{
	public class SysException : TomPITException
	{
		public SysException()
		{
		}

		public SysException(string message) : base(message)
		{
		}

		public SysException(string message, params string[] args) : base(message, args)
		{
		}

		public SysException(string message, Exception inner) : base(message, inner)
		{
		}

		public static SysException ExpectedArgument(string argument)
		{
			return new SysException(string.Format("{0} ({1}).", SR.ErrServerExpectedArgument, argument));
		}

		public static SysException InvalidArgumentType(string argument, Type expected)
		{
			return new SysException(string.Format("{0} ({1}, {2}).", SR.ErrServerInvalidArgumentType, argument, expected.Name));
		}

		public static SysException MicroServiceNotFound()
		{
			return new SysException(SR.ErrMicroServiceNotFound);
		}

		public static SysException UserNotFound()
		{
			return new SysException(SR.ErrUserNotFound);
		}
	}
}
