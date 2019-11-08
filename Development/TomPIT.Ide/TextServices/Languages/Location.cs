namespace TomPIT.Ide.TextServices.Languages
{
	public class Location : ILocation
	{
		public IRange Range { get; set; }

		public string Uri { get; set; }
	}
}
