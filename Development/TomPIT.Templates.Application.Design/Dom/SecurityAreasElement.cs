using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Dom;

namespace TomPIT.Application.Design.Dom
{
	internal class SecurityAreasElement : TomPIT.Dom.Element
	{
		public const string FolderId = "Areas";
		private List<IComponent> _areas = null;

		public SecurityAreasElement(IDomElement parent) : base(parent.Environment, parent)
		{
			Id = FolderId;
			Glyph = "fal fa-cubes";
			Title = "Areas";

			((Behavior)Behavior).AutoExpand = true;
		}

		public override bool HasChildren { get { return Areas.Count > 0; } }
		public override int ChildrenCount { get { return Areas.Count; } }

		public override void LoadChildren()
		{
			foreach (var i in Areas)
				Items.Add(new SecurityAreaElement(Environment, this, i));
		}

		public override void LoadChildren(string id)
		{
			var area = Connection.GetService<IComponentService>().SelectComponent(id.AsGuid());

			Items.Add(new SecurityAreaElement(Environment, this, area));
		}

		public List<IComponent> Areas
		{
			get
			{
				if (_areas == null)
					_areas = Connection.GetService<IComponentService>().QueryComponents(this.MicroService(), "Area").OrderBy(f => f.Name).ToList();

				return _areas;
			}
		}
	}
}
