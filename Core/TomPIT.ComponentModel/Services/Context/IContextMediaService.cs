using TomPIT.Annotations;

namespace TomPIT.Services.Context
{
	public interface IContextMediaService
	{
		string ResourceUrl([CodeAnalysisProvider(ExecutionContext.MediaProvider)]string path);
		string SanitizeText(string text);
		void CleanOrphanedResources(string existingText, string newText);
		string StripHtml(string htmlText);
	}
}
