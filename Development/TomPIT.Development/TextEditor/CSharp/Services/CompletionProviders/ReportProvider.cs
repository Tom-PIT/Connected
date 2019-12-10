using TomPIT.ComponentModel;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class ReportProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.Report;
		protected override bool IncludeReferences => true;
	}
}
