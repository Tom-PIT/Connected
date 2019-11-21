using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareMediaService
	{
		string ResourceUrl([CIP(CIP.MediaProvider)]string path);
		string SanitizeText(string text);
		void CleanOrphanedResources(string existingText, string newText);
		string StripHtml(string htmlText);
	}
}
