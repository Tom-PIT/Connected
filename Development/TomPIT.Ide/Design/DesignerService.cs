using System;
using System.Collections.Concurrent;
using TomPIT.Connectivity;

namespace TomPIT.Design
{
	internal class DesignerService : IDesignerService
	{
		private Lazy<ConcurrentDictionary<string, IPropertyEditor>> _propertyEditors = new Lazy<ConcurrentDictionary<string, IPropertyEditor>>();

		/// <summary>
		/// TODO: retrieve editors from config
		/// </summary>
		public DesignerService(ISysConnection connection)
		{
			Connection = connection;

			RegisterPropertyEditor("Text", "~/Views/Ide/Editors/Text.cshtml");
			RegisterPropertyEditor("TextArea", "~/Views/Ide/Editors/TextArea.cshtml");
			RegisterPropertyEditor("Label", "~/Views/Ide/Editors/Label.cshtml");
			RegisterPropertyEditor("Select", "~/Views/Ide/Editors/Select.cshtml");
			RegisterPropertyEditor("Check", "~/Views/Ide/Editors/Check.cshtml");
			RegisterPropertyEditor("Number", "~/Views/Ide/Editors/Number.cshtml");
			RegisterPropertyEditor("Date", "~/Views/Ide/Editors/Date.cshtml");
			RegisterPropertyEditor("Color", "~/Views/Ide/Editors/Color.cshtml");
		}

		private ISysConnection Connection { get; }

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

		private ConcurrentDictionary<string, IPropertyEditor> PropertyEditors { get { return _propertyEditors.Value; } }
	}
}
