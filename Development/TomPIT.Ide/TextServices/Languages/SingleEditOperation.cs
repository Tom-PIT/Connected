namespace TomPIT.Ide.TextServices.Languages
{
	public class SingleEditOperation : ISingleEditOperation
	{
		public bool ForceMoveMakers { get; set; }

		public IRange Range { get; set; }

		public string Text { get; set; }
	}
}
