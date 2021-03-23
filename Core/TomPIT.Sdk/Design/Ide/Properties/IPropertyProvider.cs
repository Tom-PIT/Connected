using System.Collections.Generic;

namespace TomPIT.Design.Ide.Properties
{
	public interface IPropertyProvider : IEnvironmentObject
	{
		List<string> Categories { get; }
		List<IProperty> QueryProperties(string category);

		string View { get; }
	}
}
