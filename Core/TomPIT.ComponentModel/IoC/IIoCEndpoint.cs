namespace TomPIT.ComponentModel.IoC
{
	public interface IIoCEndpoint : IText
	{
		string Container { get; }
		string Name { get; }
	}
}
