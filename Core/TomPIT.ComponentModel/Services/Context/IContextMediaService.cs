using TomPIT.Annotations;

namespace TomPIT.Services.Context
{
	public interface IContextMediaService
	{
		string ResourceUrl([CodeAnalysisProvider(ExecutionContext.MediaProvider)]string path);
	}
}
