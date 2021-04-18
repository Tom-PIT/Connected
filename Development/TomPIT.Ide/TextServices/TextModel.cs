namespace TomPIT.Ide.TextServices
{
	internal class TextModel : ITextModel
	{
		public string Id { get; set; }

		public string Uri { get; set; }
		public int Version { get; set; }
	}
}
