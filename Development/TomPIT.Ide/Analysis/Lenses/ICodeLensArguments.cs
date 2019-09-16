namespace TomPIT.Ide.Analysis.Lenses
{
	public interface ICodeLensArguments
	{
		string MicroService { get; }
		string Component { get; }
		string Element { get; }
		string Kind { get; }
	}
}
