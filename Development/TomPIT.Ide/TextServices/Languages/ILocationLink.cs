namespace TomPIT.Ide.TextServices.Languages
{
	public interface ILocationLink : ILocation
	{
		IRange OriginSelectionRange { get; }
		IRange TargetSelectionRange { get; }
	}
}
