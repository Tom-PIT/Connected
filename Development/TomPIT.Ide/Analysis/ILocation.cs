namespace TomPIT.Ide.Analysis
{
	public interface ILocation
	{
		IRange Range { get; }
		string Uri { get; }
	}
}
