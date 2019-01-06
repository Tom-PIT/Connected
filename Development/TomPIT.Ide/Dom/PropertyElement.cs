using System.Reflection;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class PropertyElement : Element
	{
		public PropertyElement(IEnvironment environment, IDomElement parent, object component, string propertyName) : base(environment, parent)
		{
			Title = propertyName;
			Id = propertyName;

			Component = component;
			Property = Component.GetType().GetProperty(propertyName);
		}

		public override object Component { get; }
		public override PropertyInfo Property { get; }
	}
}
