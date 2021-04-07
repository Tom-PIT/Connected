namespace TomPIT.Ide.TextServices.Languages
{
	public interface IDeltaDecoration
	{
		IDeltaDecorationOptions Options { get; }
		IRange Range { get; }
	}
}
