using System.Collections.Generic;
using TomPIT.Ide;

namespace TomPIT.Design
{
	public interface IPropertyProvider : IEnvironmentClient
	{
		List<string> Categories { get; }
		List<IProperty> QueryProperties(string category);
	}
}
