using TomPIT.Exceptions;

namespace TomPIT.Middleware.Interop
{
	public class ExtenderException : TomPITException
	{
		public ExtenderException(string message) : base(message)
		{
		}
	}
}
