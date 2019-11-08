namespace TomPIT.Ide.TextServices.Languages
{
	public enum SignatureHelpTriggerKind
	{
		Invoke = 1,
		TriggerCharacter = 2,
		ContentChange = 3
	}
	public interface ISignatureHelpContext
	{
		ISignatureHelp ActiveSignatureHelp { get; }
		bool IsRetrigger { get; }
		string TriggerCharacter { get; }
		SignatureHelpTriggerKind TriggerKind { get; }
	}
}
