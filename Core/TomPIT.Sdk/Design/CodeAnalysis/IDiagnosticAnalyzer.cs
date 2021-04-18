using Microsoft.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Design.CodeAnalysis
{
	public interface IDiagnosticAnalyzer
	{
		IText Text { get; }
		IMicroService MicroService { get; }
		IComponent Component { get; }
		IConfiguration Configuration { get; }
		ITenant Tenant { get; }

		DiagnosticDescriptor GetDescriptor(string id);
	}
}
