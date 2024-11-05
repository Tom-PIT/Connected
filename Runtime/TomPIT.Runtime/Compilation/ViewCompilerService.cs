using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Compilation;
internal class ViewCompilerService : IViewCompilerService
{
	public string CompileView(ITenant tenant, IText sourceCode) => ViewCompiler.Compile(tenant, sourceCode);
}
