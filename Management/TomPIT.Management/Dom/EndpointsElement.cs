using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Designers;
using TomPIT.Environment;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class EndpointsElement : Element
	{
		public const string DomId = "Endpoints";
		private EndpointsDesigner _designer = null;
		private ExistingEndpoints _instances = null;
		private PropertyInfo _property = null;

		private class ExistingEndpoints
		{
			private List<IInstanceEndpoint> _items = null;

			[Items("TomPIT.Items.EndpointsCollection, TomPIT.Management")]
			[Browsable(false)]
			public List<IInstanceEndpoint> Items
			{
				get
				{
					if (_items == null)
						_items = new List<IInstanceEndpoint>();

					return _items;
				}
			}
		}
		public EndpointsElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Id = DomId;
			Glyph = "fal fa-folder";
			Title = "Endpoints";

			((Behavior)Behavior).AutoExpand = false;
		}

		public override object Component => Endpoints;
		public override bool HasChildren { get { return Endpoints.Items.Count > 0; } }
		public override int ChildrenCount { get { return Endpoints.Items.Count; } }

		public override PropertyInfo Property
		{
			get
			{
				if (_property == null)
					_property = Component.GetType().GetProperty("Items");

				return _property;
			}
		}

		public override void LoadChildren()
		{
			foreach (var i in Existing)
				Items.Add(new EndpointElement(Environment, this, i));
		}

		public override void LoadChildren(string id)
		{
			var endpoint = Connection.GetService<IInstanceEndpointService>().Select(id.AsGuid());

			Items.Add(new EndpointElement(Environment, this, endpoint));
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new EndpointsDesigner(Environment, this);

				return _designer;
			}
		}

		private ExistingEndpoints Endpoints
		{
			get
			{
				if (_instances == null)
				{
					_instances = new ExistingEndpoints();

					var ds = Connection.GetService<IInstanceEndpointService>().Query().OrderBy(f => f.Name);

					foreach (var i in ds)
						_instances.Items.Add(i);
				}

				return _instances;
			}
		}

		public List<IInstanceEndpoint> Existing { get { return Endpoints.Items; } }
	}
}
