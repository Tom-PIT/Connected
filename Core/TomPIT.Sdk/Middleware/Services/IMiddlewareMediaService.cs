using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareMediaService
	{
		string ResourceUrl([CAP(CAP.MediaProvider)]string path);
		string SanitizeText(string text);
		void CleanOrphanedResources(string existingText, string newText);
		string StripHtml(string htmlText);
	}
}
