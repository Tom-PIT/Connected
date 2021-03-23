using System.Collections.Generic;

namespace TomPIT.Design.Ide.Designers
{
	public interface IDesignerActionResult
	{
		object Model { get; }

		InformationKind MessageKind { get; }
		string Message { get; }
		string Title { get; }

		string ExplorerPath { get; }

		Dictionary<string, string> ResponseHeaders { get; }
	}
}
