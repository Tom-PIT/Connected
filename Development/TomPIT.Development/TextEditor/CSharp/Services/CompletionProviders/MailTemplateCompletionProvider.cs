using TomPIT.ComponentModel;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class MailTemplateCompletionProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.MailTemplate;
		protected override bool IncludeReferences => true;
	}
}