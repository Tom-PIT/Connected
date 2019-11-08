namespace TomPIT.Ide.TextServices.Languages
{
	public class LocationLink : Location, ILocationLink
	{
		public IRange OriginSelectionRange { get; set; }

		public IRange TargetSelectionRange { get; set; }
	}
}
