using TomPIT.ComponentModel;

namespace TomPIT.Compilers
{
	public interface ICompiler
	{
		byte[] Compile(CompilerOptions options, IElement element);
	}
}
