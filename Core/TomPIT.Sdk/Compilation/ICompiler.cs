using TomPIT.ComponentModel;

namespace TomPIT.Compilation
{
	public interface ICompiler
	{
		byte[] Compile(CompilerOptions options, IElement element);
	}
}
