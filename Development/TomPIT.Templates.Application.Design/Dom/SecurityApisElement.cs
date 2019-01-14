using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Dom;

namespace TomPIT.Application.Design.Dom
{
	internal class SecurityApisElement : TomPIT.Dom.Element
	{
		public const string FolderId = "Api";
		private List<IComponent> _apis = null;
		private IDomDesigner _designer = null;

		public SecurityApisElement(IDomElement parent) : base(parent.Environment, parent)
		{
			Id = FolderId;
			Glyph = "fal fa-folder";
			Title = "Apis";

			((Behavior)Behavior).AutoExpand = false;
		}

		public override bool HasChildren { get { return Apis.Count > 0; } }
		public override int ChildrenCount { get { return Apis.Count; } }

		public override void LoadChildren()
		{
			foreach (var i in Apis)
				Items.Add(new SecurityApiElement(Environment, this, i));
		}

		public override void LoadChildren(string id)
		{
			var api = Connection.GetService<IComponentService>().SelectComponent(id.AsGuid());

			Items.Add(new SecurityApiElement(Environment, this, api));
		}

		public List<IComponent> Apis
		{
			get
			{
				if (_apis == null)
				{
					_apis = new List<IComponent>();

					var components = Connection.GetService<IComponentService>().QueryComponents(this.MicroService(), "Api").OrderBy(f => f.Name).ToList();

					foreach (var i in components)
					{
						var config = Connection.GetService<IComponentService>().SelectConfiguration(i.Token) as IApi;

						if (config == null || config.Scope != ElementScope.Public)
							continue;

						_apis.Add(i);
					}

				}

				return _apis;
			}
		}
	}
}
