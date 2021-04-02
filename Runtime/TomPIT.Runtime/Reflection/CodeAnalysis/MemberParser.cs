using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TomPIT.Reflection.CodeAnalysis
{
	internal class MemberParser:ParserBase
	{
		private TypeParser _typeParser;
		public MemberParser(IScriptManifestCompiler compiler):base(compiler)
		{
		}

		private TypeParser TypeParser => _typeParser ??= new TypeParser(Compiler);

		public void Resolve(MemberDeclarationSyntax syntax)
		{
			var type = TypeParser.Parse(syntax);

			if (type != null)
				Compiler.Manifest.DeclaredTypes.Add(TypeParser.Parse(type));
		}
	}
}
