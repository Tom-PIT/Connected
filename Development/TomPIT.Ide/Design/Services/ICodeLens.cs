using System.Collections.Generic;

namespace TomPIT.Design.Services
{
	public interface ICodeLens
	{
		List<ICodeLensDescriptor> Items { get; }
	}
}
