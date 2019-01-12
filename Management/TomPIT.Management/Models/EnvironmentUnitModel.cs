using TomPIT.Designers;
using TomPIT.Environment;

namespace TomPIT.Models
{
	public class EnvironmentUnitModel
	{
		public EnvironmentUnitModel(EnvironmentUnitsDesigner designer, IEnvironmentUnit environmentUnit)
		{
			Designer = designer;
			EnvironmentUnit = environmentUnit;
		}

		public IEnvironmentUnit EnvironmentUnit { get; set; }
		public EnvironmentUnitsDesigner Designer { get; }
	}
}
