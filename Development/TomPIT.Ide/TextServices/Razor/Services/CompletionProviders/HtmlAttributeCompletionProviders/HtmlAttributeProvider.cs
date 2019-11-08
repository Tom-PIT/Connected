using TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders;

namespace TomPIT.Ide.TextServices.Razor.Services.CompletionProviders.HtmlAttributeCompletionProviders
{
	internal abstract class HtmlAttributeProvider : CompletionProvider
	{
		public string AttributeName { get; set; }
	}
}
