using TomPIT.Connectivity;

namespace TomPIT.Ide.Analysis
{
	public interface IToolMiddleware
	{
		string Name { get; }
		void Execute(ITenant tenant);
	}
}
