namespace TomPIT.Ide.TextServices.Languages
{
	internal class SignatureHelpContext : ISignatureHelpContext
	{
		public ISignatureHelp ActiveSignatureHelp { get; set; }

		public bool IsRetrigger { get; set; }

		public string TriggerCharacter { get; set; }

		public SignatureHelpTriggerKind TriggerKind { get; set; }
	}
}
