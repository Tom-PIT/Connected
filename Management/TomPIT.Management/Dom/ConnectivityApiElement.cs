using System.Linq;
using System.Reflection;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Design;

namespace TomPIT.Dom
{
	internal class ConnectivityApiElement : TransactionElement
	{
		private IApi _api = null;

		public ConnectivityApiElement(IDomElement parent, IComponent api) : base(parent)
		{
			Api = api;
			Title = api.Name;
			Id = api.Token.ToString();
		}

		private IComponent Api { get; }

		public override bool HasChildren => ChildrenCount > 0;
		public override int ChildrenCount => Configuration.Operations.Count(f => !string.IsNullOrWhiteSpace(f.Name));

		public override void LoadChildren()
		{
			foreach (var i in Configuration.Operations)
			{
				if (string.IsNullOrWhiteSpace(i.Name))
					continue;

				Items.Add(new ConnectivityOperationElement(this, i));
			}
		}

		public override void LoadChildren(string id)
		{
			var op = Configuration.Operations.FirstOrDefault(f => string.Compare(f.Id.ToString(), id, true) == 0);

			if (op != null)
				Items.Add(new ConnectivityOperationElement(this, op));
		}

		public override object Component
		{
			get
			{
				if (_api == null)
					_api = Connection.GetService<IComponentService>().SelectConfiguration(Api.Token) as IApi;

				return _api;
			}
		}

		private IApi Configuration { get { return Component as IApi; } }

		public override PropertyInfo Property
		{
			get
			{
				return Configuration?.GetType().GetProperty(nameof(Configuration.Protocols));
			}
		}

		public override bool Commit(object component, string property, string attribute)
		{
			Connection.GetService<IComponentDevelopmentService>().Update(Configuration);

			return true;
		}
	}
}
