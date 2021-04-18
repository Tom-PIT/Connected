using System.Collections.Generic;

namespace TomPIT.Design
{
	public interface ITextPatchResult
	{
		string Text { get; }
		List<bool> Patches { get; }
	}
}
