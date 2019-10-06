using TomPIT.Ide.TextServices.CSharp.Services;
using TomPIT.Ide.TextServices.Services;
using TomPIT.Middleware;

namespace TomPIT.Ide.TextServices.CSharp
{
	public class CSharpEditor : CSharpEditorBase
	{
		public CSharpEditor(IMicroServiceContext context) : base(context)
		{
			Services.TryAdd(typeof(ISyntaxCheckService), new SyntaxCheckService(this));
			Services.TryAdd(typeof(ICodeActionService), new CodeActionService(this));
			Services.TryAdd(typeof(ICompletionItemService), new CompletionItemService(this));
			Services.TryAdd(typeof(IDeclarationProviderService), new DeclarationProviderService(this));
			Services.TryAdd(typeof(ISignatureHelpService), new SignatureHelpService(this));
			Services.TryAdd(typeof(IDocumentSymbolProviderService), new DocumentSymbolProviderService(this));
		}

		public override LanguageFeature Features =>
			LanguageFeature.CheckSyntax
			| LanguageFeature.CodeAction
			| LanguageFeature.CompletionItem
			| LanguageFeature.Declaration
			| LanguageFeature.SignatureHelp
			| LanguageFeature.DocumentSymbol;
	}
}
