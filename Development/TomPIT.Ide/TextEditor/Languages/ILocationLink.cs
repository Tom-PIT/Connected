namespace TomPIT.Ide.TextEditor.Languages
{
	public interface ILocationLink : ILocation
	{
		IRange OriginSelectionRange { get; }
		IRange TargetSelectionRange { get; }
	}
}
