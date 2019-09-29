namespace TomPIT.Ide.TextServices.Languages
{
	public class CompletionContext : ICompletionContext
	{
		public string TriggerCharacter { get; set; }

		public CompletionTriggerKind TriggerKind { get; set; }
	}
}
