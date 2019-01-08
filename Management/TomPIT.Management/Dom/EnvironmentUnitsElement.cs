using System.Reflection;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class EnvironmentUnitsElement : Element
	{
		public const string DomId = "EnvironmentUnits";
		private EnvironmentUnitsDesigner _designer = null;
		private PropertyInfo _property = null;

		public EnvironmentUnitsElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Id = DomId;
			Glyph = "fal fa-folder";
			Title = "Environment units";

			((Behavior)Behavior).AutoExpand = false;
		}

		public override bool HasChildren => false;

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new EnvironmentUnitsDesigner(Environment, this);

				return _designer;
			}
		}
	}
}
