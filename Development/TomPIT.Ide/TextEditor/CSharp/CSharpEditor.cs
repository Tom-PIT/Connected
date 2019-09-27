using TomPIT.Ide.TextEditor.CSharp.Services;
using TomPIT.Ide.TextEditor.Services;
using TomPIT.Middleware;

namespace TomPIT.Ide.TextEditor.CSharp
{
	public class CSharpEditor : CSharpEditorBase
	{
		public CSharpEditor(IMicroServiceContext context) : base(context)
		{
			Services.TryAdd(typeof(ISyntaxCheckService), new SyntaxCheckService(this));
			Services.TryAdd(typeof(ICodeActionService), new CodeActionService(this));
			Services.TryAdd(typeof(ICompletionItemService), new CompletionItemService(this));
			Services.TryAdd(typeof(IDeclarationProviderService), new DeclarationProviderService(this));
		}

		public override LanguageFeature Features =>
			LanguageFeature.CheckSyntax
			| LanguageFeature.CodeAction
			| LanguageFeature.CompletionItem
			| LanguageFeature.Declaration;
	}
}
