using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Development;
using TomPIT.Ide.Design;

namespace TomPIT.Design
{
	internal class DesignerService : IDesignerService
	{
		private Lazy<ConcurrentDictionary<string, IPropertyEditor>> _propertyEditors = new Lazy<ConcurrentDictionary<string, IPropertyEditor>>();

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
			RegisterPropertyEditor("Tag", "~/Views/Ide/Editors/Tag.cshtml");
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

		public List<IDevelopmentError> QueryErrors(Guid microService)
		{
			var u = Connection.CreateUrl("DevelopmentErrors", "Query");
			var e = new JObject
			{
				{"microService", microService }
			};

			return Connection.Post<List<DevelopmentError>>(u, e).ToList<IDevelopmentError>();
		}

		public void ClearErrors(Guid component, Guid element)
		{
			var u = Connection.CreateUrl("DevelopmentErrors", "Clear");
			var e = new JObject
			{
				{"component", component }
			};

			if (element != Guid.Empty)
				e.Add("element", element);

			Connection.Post(u, e);
		}

		public void InsertErrors(Guid component, List<IDevelopmentComponentError> errors)
		{
			var c = Connection.GetService<IComponentService>().SelectComponent(component);

			if (c == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			var u = Connection.CreateUrl("DevelopmentErrors", "Insert");
			var e = new JObject
			{
				{"microService", c.MicroService },
				{"component", c.Token }
			};

			var a = new JArray();
			e.Add("errors", a);

			foreach(var error in errors)
			{
				var je = new JObject
				{
					{"message", error.Message },
					{"severity", (int)error.Severity },
				};

				if (error.Element != Guid.Empty)
					je.Add("element", error.Element);

				a.Add(je);
			}

			Connection.Post(u, e);
		}

		private ConcurrentDictionary<string, IPropertyEditor> PropertyEditors { get { return _propertyEditors.Value; } }
	}
}
