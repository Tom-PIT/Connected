using System.Reflection;
using TomPIT.Design.Ide.Dom;

namespace TomPIT.Ide.Dom
{
	public class PropertyElement : DomElement
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
