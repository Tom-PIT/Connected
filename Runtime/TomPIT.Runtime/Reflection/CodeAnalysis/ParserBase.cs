namespace TomPIT.Reflection.CodeAnalysis
{
	internal abstract class ParserBase
	{
		protected ParserBase(IScriptManifestCompiler compiler)
		{
			Compiler = compiler;
		}

		protected IScriptManifestCompiler Compiler { get; }
	}
}
