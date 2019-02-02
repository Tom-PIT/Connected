using TomPIT.ComponentModel;
using TomPIT.Design;

namespace TomPIT.Dom
{
	public class TypeExceptionElement : Element
	{
		public TypeExceptionElement(IDomElement parent, IComponent component) : base(parent)
		{
			Target = component;
			Id = Target.Token.ToString();
			Glyph = "fal fa-times-circle dev-explorer-node-danger";
			Title = Target.Name;

			((Behavior)Behavior).AutoExpand = false;
			((Behavior)Behavior).Static = false;

			Verbs.Add(new Verb
			{
				Action = VerbAction.Ide,
				Confirm = string.Format("Are you sure you want to delete component '{0}'?", Title),
				Id = "deleteComponent",
				Name = "Delete component"
			});

		}

		private IComponent Target { get; }
		public override object Component => Target;

	}
}
