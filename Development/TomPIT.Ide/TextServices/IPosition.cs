namespace TomPIT.Ide.TextServices
{
	public interface IPosition
	{
		int Column { get; }
		int LineNumber { get; }
	}
}
