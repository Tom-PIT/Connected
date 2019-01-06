using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class CategoryElement : TomPIT.Dom.Element, IComponentsElement
	{
		private List<IComponent> _components = null;
		private IDomDesigner _designer = null;

		public CategoryElement(IEnvironment environment, IDomElement parent, string category, string id, string title, string glyph) : base(environment, parent)
		{
			Id = id;
			Title = title;
			Category = category;
			Glyph = glyph;
		}

		public string Category { get; }

		public override object Component => Existing;
		public override int ChildrenCount => Existing.Count;
		public override bool HasChildren => Existing.Count > 0;

		public override void LoadChildren()
		{
			foreach (var i in Existing.OrderBy(f => f.Name))
				Items.Add(i.GetDomElement(Environment, this));
		}

		public override void LoadChildren(string id)
		{
			var ds = Existing.FirstOrDefault(f => f.Token == id.AsGuid());

			Items.Add(ds.GetDomElement(Environment, this));
		}

		public List<IComponent> Existing
		{
			get
			{
				if (_components == null)
				{
					var fs = DomQuery.Closest<IFeatureScope>(this);
					var fid = fs == null
						? Guid.Empty
						: fs.Feature;

					_components = Connection.GetService<IComponentService>().QueryComponents(Environment.Context.MicroService(), Category).Where(f => f.Feature == fid).ToList();
				}

				return _components;
			}
		}

		public List<IItemDescriptor> Descriptors
		{
			get
			{
				var service = Connection.GetService<IMicroServiceService>().Select(Environment.Context.MicroService());

				if (service == null)
					throw new IdeException(SR.ErrMicroServiceNotFound);

				var template = Connection.GetService<IMicroServiceTemplateService>().Select(service.Template);

				if (template == null)
					throw new IdeException(SR.ErrMicroServiceTemplateNotFound);

				return template.QueryDescriptors(this, Category);
			}
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new ComponentsDesigner(Environment, this);

				return _designer;
			}
		}
	}
}
