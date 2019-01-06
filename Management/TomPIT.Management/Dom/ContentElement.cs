using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	internal class ContentElement : Element
	{
		public const string DomId = "{879FF869-E547-410C-877B-BC3003AC4A63}";
		private List<IComponent> _components = null;

		public ContentElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Title = SR.DomContent;
			Id = DomId;
		}

		public override bool HasChildren { get { return Components.Count > 0; } }

		public override void LoadChildren()
		{
			foreach (var i in Components)
				Items.Add(new DataManagementElement(Environment, this, i));
		}

		public override void LoadChildren(string id)
		{
			var c = SysContext.GetService<IComponentService>().SelectComponent(id.AsGuid());

			if (c != null)
				Items.Add(new DataManagementElement(Environment, this, c));
		}

		public List<IComponent> Components
		{
			get
			{
				if (_components == null)
					_components = SysContext.GetService<IComponentService>().QueryComponents(DomQuery.Closest<IMicroServiceScope>(this).MicroService.Token, "DataManagement");

				return _components;
			}
		}
	}
}
