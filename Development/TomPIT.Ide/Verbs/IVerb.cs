namespace TomPIT.Ide.Verbs
{
	public enum VerbAction
	{
		Designer = 1,
		Ide = 2
	}

	public interface IVerb
	{
		string Name { get; }
		string Id { get; }
		string Confirm { get; }
		VerbAction Action { get; }
	}
}
