namespace TomPIT.Design.Services
{
	public interface ICodeCompletionService
	{
		ICodeCompletionProvider GetProvider(string language);
	}
}
