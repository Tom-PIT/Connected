using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Design;
using TomPIT.Configuration;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide;
using TomPIT.Ide.Dom.ComponentModel;
using TomPIT.Management.Designers;
using TomPIT.Management.Items;

namespace TomPIT.Management.Dom
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

		public SettingsElement(IDomElement parent) : base(parent)
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
					_designer = new SettingsDesigner(this);

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
					var items = Environment.Context.Tenant.GetService<ISettingService>().Query().Where(f => f != null && string.IsNullOrEmpty(f.Type) && string.IsNullOrEmpty(f.PrimaryKey));

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

			Environment.Context.Tenant.GetService<ISettingService>().Update(s.Name, null, null, null, s.Value);

			return true;
		}
	}
}
