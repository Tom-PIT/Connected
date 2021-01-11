namespace TomPIT.Runtime
{
	public enum RuntimeUrlKind
	{
		Other = 0,
		Default = 1
	}
	public interface IRuntimeUrl
	{
		string Url { get; }
		int Weight { get; }
	}
}
