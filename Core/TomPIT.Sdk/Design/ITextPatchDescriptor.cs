using System.Collections.Generic;

namespace TomPIT.Design
{
	public interface ITextPatchDescriptor
	{
		public List<ITextDiffDescriptor> Diffs { get; }
		int Start1 { get; set; }
		int Start2 { get; set; }
		int Length1 { get; set; }
		int Length2 { get; set; }
	}
}
