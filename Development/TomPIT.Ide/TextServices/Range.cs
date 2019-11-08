namespace TomPIT.Ide.TextServices
{
	internal class Range : IRange
	{
		public int EndColumn { get; set; }

		public int EndLineNumber { get; set; }

		public int StartColumn { get; set; }

		public int StartLineNumber { get; set; }
	}
}
