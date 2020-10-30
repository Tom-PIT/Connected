namespace TomPIT.Quality
{
	public enum TestMessageSeverity
	{
		Debug = 1,
		Info = 2,
		Warning = 3,
		Error = 4
	}
	public interface IUnitTestMessage
	{
		TestMessageSeverity Severity { get; }
		string Message { get; }
	}
}
