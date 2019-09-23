namespace TomPIT.Ide.TextEditor.Languages
{
	public class Location : ILocation
	{
		public IRange Range { get; set; }

		public string Uri { get; set; }
	}
}
