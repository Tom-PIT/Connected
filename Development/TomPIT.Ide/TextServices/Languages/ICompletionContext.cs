namespace TomPIT.Ide.TextServices.Languages
{
	public enum CompletionTriggerKind
	{
		Invoke = 0,
		TriggerCharacter = 1,
		TriggerForIncompleteCompletions = 2
	}
	public interface ICompletionContext
	{
		string TriggerCharacter { get; }
		CompletionTriggerKind TriggerKind { get; }
	}
}
