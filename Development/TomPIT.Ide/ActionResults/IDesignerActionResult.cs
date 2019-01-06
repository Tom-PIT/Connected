namespace TomPIT.ActionResults
{
	public interface IDesignerActionResult
	{
		object Model { get; }

		InformationKind MessageKind { get; }
		string Message { get; }
		string Title { get; }

		string ExplorerPath { get; }
	}
}
