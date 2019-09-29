using System.Collections.Generic;

namespace TomPIT.Ide.TextServices
{
	public enum MarkerSeverity
	{
		Hint = 1,
		Info = 2,
		Warning = 4,
		Error = 8
	}

	public enum MarkerTag
	{
		Unnecessary = 1,
		Deprecated = 2
	}
	public interface IMarkerData
	{
		string Code { get; }
		int EndColumn { get; }
		int EndLineNumber { get; }
		string Message { get; }
		List<IRelatedInformation> RelatedInformation { get; }
		MarkerSeverity Severity { get; }
		string Source { get; }
		int StartColumn { get; }
		int StartLineNumber { get; }
		List<MarkerTag> Tags { get; }
	}
}
