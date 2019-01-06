namespace TomPIT.ComponentModel
{
	public interface IServiceReference
	{
		string MicroService { get; }

		bool IsValid { get; }
	}
}
