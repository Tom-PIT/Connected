using System.Collections.Generic;

namespace TomPIT.Design
{
	internal class TextDiff : ITextDiff
	{
		public ITextPatchResult Apply(List<ITextPatchDescriptor> descriptors, string baseText)
		{
			return TextDiffProcessor.Apply(descriptors, baseText is null ? string.Empty: baseText);
		}

		public ITextPatchResult Apply(string original, string modified, string baseText)
		{
			var diffs = Diff(original is null ? string.Empty : original, modified is null ? string.Empty: modified);
			var patch = Patch(diffs);
			
			return Apply(patch, baseText is null ? string.Empty: baseText);
		}
		public List<ITextDiffDescriptor> Diff(string original, string modified)
		{
			return TextDiffProcessor.Diff(new TextDiffArgs
			{
				Original = original is null ? string.Empty: original,
				Modified = modified is null ? string.Empty: modified,
				Timeout = 0,
				Mode =  TextDiffCompareMode.Line
			});
		}

		public List<ITextDiffDescriptor> Diff(string original, string modified, TextDiffCompareMode mode)
		{
			return TextDiffProcessor.Diff(new TextDiffArgs
			{
				Original = original is null ? string.Empty : original,
				Modified = modified is null ? string.Empty : modified,
				Timeout = 0,
				Mode = mode
			});
		}

		public List<ITextPatchDescriptor> Patch(List<ITextDiffDescriptor> diffs)
		{
			return TextDiffProcessor.Patch(diffs);
		}

		public string Render(List<ITextDiffDescriptor> diffs)
		{
			return TextDiffProcessor.Render(diffs);
		}
	}
}
