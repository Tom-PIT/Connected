using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class ConnectivityElement : Element
	{
		public const string DomId = "Connectivity";
		private List<IComponent> _root = null;
		private IDomDesigner _designer = null;

		public ConnectivityElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Id = DomId;
			Glyph = "fal fa-folder";
			Title = "Connectivity";
		}

		public override bool HasChildren { get { return Apis != null && Apis.Count > 0; } }

		public override void LoadChildren()
		{
			if (Apis == null)
				return;

			foreach (var i in Apis)
				Items.Add(new ConnectivityApiElement(Environment, this, i));
		}

		public override void LoadChildren(string id)
		{
			if (Apis == null)
				return;

			var d = Apis.FirstOrDefault(f => f.Token == id.AsGuid());

			if (d != null)
				Items.Add(new ConnectivityApiElement(Environment, this, d));
		}

		private IMicroService MicroService { get { return DomQuery.Closest<IMicroServiceScope>(this).MicroService; } }

		private List<IComponent> Apis
		{
			get
			{
				if (_root == null)
					_root = Connection.GetService<IComponentService>().QueryComponents(MicroService.Token, "Api");

				return _root;
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
