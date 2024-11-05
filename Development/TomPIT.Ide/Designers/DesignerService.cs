using System;
using System.Collections.Concurrent;
using TomPIT.Connectivity;
using TomPIT.Design.Ide.Properties;
using TomPIT.Design.Tools;
using TomPIT.Ide.Properties;
using TomPIT.Middleware;

namespace TomPIT.Ide.Designers
{
	internal class DesignerService : TenantObject, IDesignerService
	{
		private const string Controller = "ComponentDevelopment";
		private Lazy<ConcurrentDictionary<string, IPropertyEditor>> _propertyEditors = new Lazy<ConcurrentDictionary<string, IPropertyEditor>>();
		private Lazy<ConcurrentBag<IAutoFixProvider>> _autoFixProviders = new Lazy<ConcurrentBag<IAutoFixProvider>>();

		public DesignerService(ITenant tenant) : base(tenant)
		{
			RegisterPropertyEditor("Text", "~/Views/Ide/Editors/Text.cshtml");
			RegisterPropertyEditor("TextArea", "~/Views/Ide/Editors/TextArea.cshtml");
			RegisterPropertyEditor("Label", "~/Views/Ide/Editors/Label.cshtml");
			RegisterPropertyEditor("Select", "~/Views/Ide/Editors/Select.cshtml");
			RegisterPropertyEditor("Check", "~/Views/Ide/Editors/Check.cshtml");
			RegisterPropertyEditor("Number", "~/Views/Ide/Editors/Number.cshtml");
			RegisterPropertyEditor("Date", "~/Views/Ide/Editors/Date.cshtml");
			RegisterPropertyEditor("Color", "~/Views/Ide/Editors/Color.cshtml");
			RegisterPropertyEditor("Tag", "~/Views/Ide/Editors/Tag.cshtml");
		}

		public IPropertyEditor GetPropertyEditor(string name)
		{
			if (PropertyEditors.TryGetValue(name, out IPropertyEditor r))
				return r;

			return null;
		}

		public void RegisterPropertyEditor(string name, string view)
		{
			var e = new PropertyEditor
			{
				Name = name,
				View = view
			};

			PropertyEditors.TryAdd(name, e);
		}

		private string CreateUrl(string action)
		{
			return Tenant.CreateUrl(Controller, action);
		}
		private ConcurrentDictionary<string, IPropertyEditor> PropertyEditors => _propertyEditors.Value;
	}
}
