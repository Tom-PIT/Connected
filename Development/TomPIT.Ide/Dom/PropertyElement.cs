using System.Reflection;

namespace TomPIT.Dom
{
	public class PropertyElement : Element
	{
		public PropertyElement(IDomElement parent, object component, string propertyName) : base(parent)
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
