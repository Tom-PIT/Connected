using System.Collections.Generic;

namespace TomPIT.Design.Services
{
	public interface ISuggestion
	{
		string Label { get; }
		int Kind { get; }
		string Description { get; }
		string InsertText { get; }
		string SortText { get; }
		string FilterText { get; }

		List<string> CommitCharacters { get; }
	}
}
