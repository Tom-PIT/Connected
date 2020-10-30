using TomPIT.ComponentModel;

namespace TomPIT.Development.TextEditor.CSharp.Services.CompletionProviders
{
	internal class SettingMiddlewareProvider : ComponentCompletionProvider
	{
		protected override string ComponentCategory => ComponentCategories.Settings;
		protected override bool IncludeReferences => true;
	}
}
