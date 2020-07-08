namespace TomPIT.Ide.Designers
{
	public class ModelDescriptor
	{
		public string Language { get; set; }
		public string FileName { get; set; }
		public string Text { get; set; }
		public bool CodeAction { get; set; }
		public bool CodeCompletion { get; set; }
		public bool Declaration { get; set; }
		public bool Definition { get; set; }
		public bool SignatureHelp { get; set; }
		public bool DocumentSymbol { get; set; }
		public string MicroService { get; set; }
	}
}
