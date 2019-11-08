using System.Collections.Generic;
using TomPIT.Ide.Properties;

namespace TomPIT.Ide.Environment.Providers
{
	public interface IPropertyProvider : IEnvironmentObject
	{
		List<string> Categories { get; }
		List<IProperty> QueryProperties(string category);

		string View { get; }
	}
}
