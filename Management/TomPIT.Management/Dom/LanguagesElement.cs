using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Design;
using TomPIT.Globalization;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Environment;
using TomPIT.Management.Designers;
using TomPIT.Management.Items;

namespace TomPIT.Management.Dom
{
	internal class LanguagesElement : DomElement
	{
		private ExistingLanguages _ds = null;
		public const string FolderId = "Languages";
		private LanguagesDesigner _designer = null;
		private PropertyInfo _property = null;

		private class ExistingLanguages
		{
			private List<ILanguage> _items = null;

			[Items(typeof(LanguagesCollection))]
			[Browsable(false)]
			public List<ILanguage> Items
			{
				get
				{
					if (_items == null)
						_items = new List<ILanguage>();

					return _items;
				}
			}
		}

		public LanguagesElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Id = FolderId;
			Glyph = "fal fa-folder";
			Title = "Languages";

			((Behavior)Behavior).AutoExpand = true;
		}

		public override object Component => Languages;
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
				Items.Add(new LanguageElement(this, i));
		}

		public override void LoadChildren(string id)
		{
			var d = Existing.FirstOrDefault(f => f.Token == new Guid(id));

			if (d != null)
				Items.Add(new LanguageElement(this, d));
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new LanguagesDesigner(this);

				return _designer;
			}
		}

		private ExistingLanguages Languages
		{
			get
			{
				if (_ds == null)
				{
					_ds = new ExistingLanguages();

					var items = Environment.Context.Tenant.GetService<ILanguageService>().Query();

					if (items != null)
						items = items.OrderBy(f => f.Name).ToList();

					foreach (var i in items)
						_ds.Items.Add(i);
				}

				return _ds;
			}
		}

		public List<ILanguage> Existing { get { return Languages.Items; } }

	}
}
