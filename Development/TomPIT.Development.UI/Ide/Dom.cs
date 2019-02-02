using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Ide
{
	public class Dom : DomBase
	{
		public Dom(IEnvironment environment, string path) : base(environment, path)
		{
			Initialize();
		}

		public override List<IDomElement> Root()
		{
			return new List<IDomElement> { new MicroServiceElement(Environment, Environment.Context.MicroService()) };
		}

		public override List<IItemDescriptor> ProvideAddItems(IDomElement selection)
		{
			if (selection == null)
				return null;

			var ms = DomQuery.Closest<IMicroServiceScope>(selection);

			if (ms == null)
				return null;

			var template = Environment.Context.Connection().GetService<IMicroServiceTemplateService>().Select(ms.MicroService.Template);

			if (template == null)
				return null;

			var r = template.ProvideAddItems(selection);

			if (r == null)
				r = new List<IItemDescriptor>();

			AddStaticItems(r);

			return r;
		}

		private void AddStaticItems(List<IItemDescriptor> items)
		{
			var e = Environment.Selection.Element;

			if (e == null)
				return;

			items.Insert(0, new ItemDescriptor("Folder", "Folder", typeof(Folder)) { Category = "Microservice", Glyph = "fal fa-folder" });
		}

		public void SetPath(string path)
		{
			SelectedPath = path;
			Initialize();
		}
	}
}
