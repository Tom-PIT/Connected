namespace TomPIT.Ide.TextEditor.Languages
{
	public class CompletionContext : ICompletionContext
	{
		public string TriggerCharacter { get; set; }

		public CompletionTriggerKind TriggerKind { get; set; }
	}
}
