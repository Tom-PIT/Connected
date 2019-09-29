namespace TomPIT.Ide.TextServices.Languages
{
	public interface ILocation
	{
		IRange Range { get; }
		string Uri { get; }
	}
}
