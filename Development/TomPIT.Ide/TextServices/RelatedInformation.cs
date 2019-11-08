namespace TomPIT.Ide.TextServices
{
	internal class RelatedInformation : IRelatedInformation
	{
		public int EndColumn { get; set; }

		public int EndLineNumber { get; set; }

		public string Message { get; set; }

		public string Uri { get; set; }

		public int StartColumn { get; set; }

		public int StartLineNumber { get; set; }
	}
}
