using System.Collections.Generic;
using TomPIT.Designers;
using TomPIT.Environment;

namespace TomPIT.Models
{
	public class EnvironmentUnitsModel
	{
		public EnvironmentUnitsModel(EnvironmentUnitsDesigner designer, List<IEnvironmentUnit> environmentUnits)
		{
			Designer = designer;
			EnvironmentUnits = environmentUnits;
		}

		public List<IEnvironmentUnit> EnvironmentUnits { get; set; }
		public EnvironmentUnitsDesigner Designer { get; }
	}
}
