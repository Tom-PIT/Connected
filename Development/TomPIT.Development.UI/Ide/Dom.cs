using System.Collections.Generic;
using System.Linq;
using TomPIT.Design;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Ide.Collections;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Dom;

namespace TomPIT.Development.Ide
{
	public class Dom : DomRoot
	{
		public Dom(IEnvironment environment, string path) : base(environment, path)
		{
			Initialize();
		}

		public override List<IDomElement> Root()
		{
			return new List<IDomElement> { new MicroServiceElement(Environment, Environment.Context.MicroService.Token) };
		}

		public override List<IItemDescriptor> ProvideAddItems(IDomElement selection)
		{
			if (selection == null)
				return null;

			var ms = DomQuery.Closest<IMicroServiceScope>(selection);

			if (ms == null)
				return null;

			var template = Environment.Context.Tenant.GetService<IMicroServiceTemplateService>().Select(ms.MicroService.Template);

			if (template == null)
				return null;

			var addItems = template.ProvideAddItems(selection);
			var r = new List<IItemDescriptor>();

			if (addItems != null && addItems.Count > 0)
				r.AddRange(addItems);

			var templates = Environment.Context.Tenant.GetService<IMicroServiceTemplateService>().Query().Where(f => f.Kind == TemplateKind.Plugin && f.Token != ms.MicroService.Template);

			foreach (var t in templates)
			{
				addItems = t.ProvideGlobalAddItems(selection);

				if (addItems != null && addItems.Count > 0)
					r.AddRange(addItems);
			}

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
