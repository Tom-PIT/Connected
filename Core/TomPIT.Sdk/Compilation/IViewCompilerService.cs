using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Compilation;
public interface IViewCompilerService
{
	string CompileView(ITenant tenant, IText sourceCode);
}
