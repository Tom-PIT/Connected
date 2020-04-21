namespace TomPIT.Ide.TextServices.Languages
{
	public interface IWorkspaceEditMetadata
	{
		string Description { get; }
		string Label { get; }
		bool NeedsConfirmation { get; }
		string IconPath { get; }
	}
}
