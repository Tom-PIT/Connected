using System.Collections.Generic;

namespace TomPIT.Ide.Analysis.Lenses
{
	public interface ICodeLens
	{
		List<ICodeLensDescriptor> Items { get; }
	}
}
