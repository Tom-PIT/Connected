using System.Reflection;

namespace TomPIT.Exceptions
{
	public class DiagnosticDescriptor
	{
		public MethodBase Method { get; set; }
		public int Line { get; set; }
	}
}
