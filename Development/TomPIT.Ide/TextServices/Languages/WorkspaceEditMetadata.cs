namespace TomPIT.Ide.TextServices.Languages
{
	internal class WorkspaceEditMetadata : IWorkspaceEditMetadata
	{
		public string Description { get; set; }

		public string Label { get; set; }

		public bool NeedsConfirmation { get; set; }

		public string IconPath { get; set; }
	}
}
