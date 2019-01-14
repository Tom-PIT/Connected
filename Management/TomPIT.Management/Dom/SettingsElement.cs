using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Configuration;
using TomPIT.Designers;
using TomPIT.Ide;
using TomPIT.Items;

namespace TomPIT.Dom
{
	public class SettingsElement : TransactionElement
	{
		private ExistingSettings _ds = null;
		private SettingsDesigner _designer = null;
		private PropertyInfo _property = null;

		private class ExistingSettings
		{
			private List<ISetting> _items = null;

			[Items(typeof(SettingsCollection))]
			[Browsable(false)]
			public List<ISetting> Items
			{
				get
				{
					if (_items == null)
						_items = new List<ISetting>();

					return _items;
				}
			}
		}

		public const string ElementId = "Settings";

		public SettingsElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Glyph = "fal fa-folder";
			Title = "Settings";
			Id = ElementId;
		}

		public override object Component => Settings;

		public override bool HasChildren => false;
		public override PropertyInfo Property
		{
			get
			{
				if (_property == null)
					_property = Settings.GetType().GetProperty("Items");

				return _property;
			}
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new SettingsDesigner(Environment, this);

				return _designer;
			}
		}

		private ExistingSettings Settings
		{
			get
			{
				if (_ds == null)
				{
					_ds = new ExistingSettings();

					var rg = DomQuery.Closest<IResourceGroupScope>(this);
					var items = Connection.GetService<ISettingService>().Query(rg == null ? Guid.Empty : rg.ResourceGroup.Token);

					if (items != null)
						items = items.OrderBy(f => f.Name).ToList();

					foreach (var i in items)
						_ds.Items.Add(i);
				}

				return _ds;
			}
		}

		public List<ISetting> Existing { get { return Settings.Items; } }

		public override bool Commit(object component, string property, string attribute)
		{
			var s = component as ISetting;

			Connection.GetService<ISettingManagementService>().Update(s.ResourceGroup, s.Name, s.Value, s.Visible, s.DataType, s.Tags);

			return true;
		}
	}
}
