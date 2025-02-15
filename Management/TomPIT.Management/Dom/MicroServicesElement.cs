﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide;
using TomPIT.Ide.Dom;
using TomPIT.Management.ComponentModel;
using TomPIT.Management.Designers;
using TomPIT.Management.Items;

namespace TomPIT.Management.Dom
{
	internal class MicroServicesElement : DomElement
	{
		private ExistingMicroServices _ds = null;
		private MicroServicesDesigner _designer = null;
		private PropertyInfo _property = null;

		private class ExistingMicroServices
		{
			private List<IMicroService> _items = null;

			[Items(typeof(MicroServicesCollection))]
			[Browsable(false)]
			public List<IMicroService> Items
			{
				get
				{
					if (_items == null)
						_items = new List<IMicroService>();

					return _items;
				}
			}
		}

		public const string ElementId = "RgMicroServices";

		public MicroServicesElement(IDomElement parent) : base(parent)
		{
			Glyph = "fal fa-shield-check";
			Title = SR.DomMicroServices;
			Id = ElementId;
		}

		public override object Component => MicroServices;
		public override bool HasChildren { get { return Existing.Count > 0; } }
		public override int ChildrenCount => Existing.Count;
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
				Items.Add(new MicroServiceManagementElement(this, i));
		}

		public override void LoadChildren(string id)
		{
			var d = Existing.FirstOrDefault(f => f.Token == new Guid(id));

			if (d != null)
				Items.Add(new MicroServiceManagementElement(this, d));
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new MicroServicesDesigner(this);

				return _designer;
			}
		}

		private ExistingMicroServices MicroServices
		{
			get
			{
				if (_ds == null)
				{
					_ds = new ExistingMicroServices();

					var items = Environment.Context.Tenant.GetService<IMicroServiceManagementService>().Query(DomQuery.Closest<IResourceGroupScope>(this).ResourceGroup.Token);

					if (items != null)
						items = items.OrderBy(f => f.Name).ToList();

					foreach (var i in items)
						_ds.Items.Add(i);
				}

				return _ds;
			}
		}

		public List<IMicroService> Existing { get { return MicroServices.Items; } }

	}
}
