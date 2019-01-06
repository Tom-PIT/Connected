using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Design
{
	public class EnvironmentUnitsDesigner : DomDesigner<EnvironmentUnitsElement>
	{
		public EnvironmentUnitsDesigner(IEnvironment environment, EnvironmentUnitsElement element) : base(environment, element)
		{
		}

		public override string View => "~/Views/Ide/Designers/EnvironmentUnits.cshtml";
	}
}
