using System.Collections.Generic;

namespace TomPIT.Dom
{
	public interface IDom
	{
		List<IDomElement> Query(string path, int depth);
		IDomElement Select(string path, int depth);

		List<IDomElement> CreateDomTree(string path);
	}
}
