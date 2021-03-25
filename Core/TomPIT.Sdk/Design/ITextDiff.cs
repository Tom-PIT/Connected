using System.Collections.Generic;

namespace TomPIT.Design
{
	public enum TextDiffOperation
	{
		Delete = 1,
		Insert = 2,
		Equal = 3
	}

	public enum TextDiffCompareMode
	{
		Char = 1,
		Word = 2,
		Line = 3
	}

	public interface ITextDiff
	{
		List<ITextDiffDescriptor> Diff(string original, string modified);
		List<ITextDiffDescriptor> Diff(string original, string modified, TextDiffCompareMode mode);
		List<ITextPatchDescriptor> Patch(List<ITextDiffDescriptor> diffs);
		ITextPatchResult Apply(List<ITextPatchDescriptor> descriptors, string baseText);
		string Render(List<ITextDiffDescriptor> diffs);
	}
}
