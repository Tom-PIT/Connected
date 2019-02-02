using System.Collections.Generic;
using TomPIT.Design;

namespace TomPIT.Dom
{
	public interface IDom
	{
		List<IDomElement> Query(string path, int depth);
		IDomElement Select(string path, int depth);

		List<IDomElement> CreateDomTree(string path);
		List<IItemDescriptor> ProvideAddItems(IDomElement selection);
	}
}
