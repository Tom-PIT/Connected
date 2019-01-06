namespace TomPIT.Design
{
	public interface ICodeLensArguments
	{
		string MicroService { get; }
		string Component { get; }
		string Element { get; }
		string Kind { get; }
	}
}
