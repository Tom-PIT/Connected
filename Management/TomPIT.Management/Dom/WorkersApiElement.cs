using System;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	internal class WorkersApiElement : Element
	{
		private IApi _api = null;

		public WorkersApiElement(IEnvironment environment, IDomElement parent, IComponent api) : base(environment, parent)
		{
			ComponentApi = api;

			Glyph = "fal fa-plug";
			Title = api.Name;
			Id = api.Token.ToString();
		}

		private IComponent ComponentApi { get; }

		public override bool HasChildren => Api != null && Api.Operations.Count > 0;

		private IApi Api
		{
			get
			{
				if (_api == null)
					_api = SysContext.GetService<IComponentService>().SelectConfiguration(ComponentApi.Token) as IApi;

				return _api;
			}
		}

		public override void LoadChildren()
		{
			if (Api == null)
				return;

			foreach (var i in Api.Operations.OrderBy(f => f.Name))
				Items.Add(new WorkersApiOperationElement(Environment, this, i));
		}

		public override void LoadChildren(string id)
		{
			if (Api == null)
				return;

			var d = Api.Operations.FirstOrDefault(f => f.Id.ToString().Equals(id, StringComparison.OrdinalIgnoreCase));

			if (d == null)
				return;

			Items.Add(new WorkersApiOperationElement(Environment, this, d));
		}
	}
}
