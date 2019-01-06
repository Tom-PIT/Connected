using TomPIT.Application.Apis;
using TomPIT.Application.Events;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Application.Dom
{
	internal class ModelElement : Element
	{
		public const string ElementId = "Model";

		public ModelElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Id = ElementId;
			Glyph = "fal fa-exchange-alt";
			Title = "Model";
		}

		public override bool HasChildren => true;

		public override void LoadChildren()
		{
			LoadInterfaces();
			LoadEvents();
			LoadEventHandlers();
		}

		private void LoadEvents()
		{
			Items.Add(new CategoryElement(Environment, this, DistributedEvent.ComponentCategory, "Events", "Events", "fal fa-folder"));
		}

		private void LoadEventHandlers()
		{
			Items.Add(new CategoryElement(Environment, this, EventHandler.ComponentCategory, "Event handlers", "Event handlers", "fal fa-folder"));
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, "Apis", true) == 0)
				LoadInterfaces();
			else if (string.Compare(id, "Events", true) == 0)
				LoadEvents();
			else if (string.Compare(id, "Event handlers", true) == 0)
				LoadEventHandlers();
		}

		private void LoadInterfaces()
		{
			Items.Add(new CategoryElement(Environment, this, Api.ComponentCategory, "Apis", "Apis", "fal fa-exchange-alt"));
		}
	}
}
