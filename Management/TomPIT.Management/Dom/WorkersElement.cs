using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class WorkersElement : Element
	{
		public const string DomId = "Workers";
		private List<IComponent> _workers = null;
		private IDomDesigner _designer = null;

		public WorkersElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Id = DomId;
			Glyph = "fal fa-folder";
			Title = SR.DomWorkers;

			((Behavior)Behavior).AutoExpand = false;
		}

		public override bool HasChildren { get { return Workers != null && Workers.Count > 0; } }

		public override void LoadChildren()
		{
			if (Workers == null)
				return;

			foreach (var i in Workers)
				Items.Add(new WorkersApiElement(Environment, this, i));
		}

		public override void LoadChildren(string id)
		{
			if (Workers == null)
				return;

			var d = Workers.FirstOrDefault(f => f.Token == id.AsGuid());

			if (d != null)
				Items.Add(new WorkersApiElement(Environment, this, d));
		}

		private IMicroService MicroService { get { return DomQuery.Closest<IMicroServiceScope>(this).MicroService; } }

		private List<IComponent> Workers
		{
			get
			{
				if (_workers == null)
					_workers = SysContext.GetService<IComponentService>().QueryComponents(MicroService.Token, "Api");

				return _workers;
			}
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new EmptyDesigner(Environment, this);

				return _designer;
			}
		}
	}
}
