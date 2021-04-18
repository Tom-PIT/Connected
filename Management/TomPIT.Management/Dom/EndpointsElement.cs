using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Design;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Environment;
using TomPIT.Ide.Dom;
using TomPIT.Management.Designers;

namespace TomPIT.Management.Dom
{
	public class EndpointsElement : DomElement
	{
		public const string DomId = "Endpoints";
		private EndpointsDesigner _designer = null;
		private ExistingEndpoints _instances = null;
		private PropertyInfo _property = null;

		private class ExistingEndpoints
		{
			private List<IInstanceEndpoint> _items = null;

			[Items("TomPIT.Management.Items.EndpointsCollection, TomPIT.Management")]
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
		public EndpointsElement(IDomElement parent) : base(parent)
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
				Items.Add(new EndpointElement(this, i));
		}

		public override void LoadChildren(string id)
		{
			var endpoint = Environment.Context.Tenant.GetService<IInstanceEndpointService>().Select(new System.Guid(id));

			Items.Add(new EndpointElement(this, endpoint));
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new EndpointsDesigner(this);

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

					var ds = Environment.Context.Tenant.GetService<IInstanceEndpointService>().Query().OrderBy(f => f.Name);

					foreach (var i in ds)
						_instances.Items.Add(i);
				}

				return _instances;
			}
		}

		public List<IInstanceEndpoint> Existing { get { return Endpoints.Items; } }
	}
}
