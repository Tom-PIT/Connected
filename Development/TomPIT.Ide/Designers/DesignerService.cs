using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design.Tools;
using TomPIT.Development;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Ide.Analysis.Diagnostics;
using TomPIT.Ide.Properties;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Ide.Designers
{
	internal class DesignerService : TenantObject, IDesignerService
	{
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

		public IDevelopmentComponentError SelectError(Guid identifier)
		{
			var u = Tenant.CreateUrl("DevelopmentErrors", "Select");
			var e = new JObject();

			e.Add("identifier", identifier);

			return Tenant.Post<DevelopmentComponentError>(u, e);
		}

		public List<IDevelopmentComponentError> QueryErrors(Guid microService, ErrorCategory category)
		{
			var u = Tenant.CreateUrl("DevelopmentErrors", "Query");
			var e = new JObject();

			if (microService != Guid.Empty)
				e.Add("microService", microService);

			if (category != ErrorCategory.NotSet)
				e.Add("category", category.ToString());

			return Tenant.Post<List<DevelopmentComponentError>>(u, e).ToList<IDevelopmentComponentError>();
		}

		public List<IDevelopmentComponentError> QueryErrors(ErrorCategory category)
		{
			return QueryErrors(Guid.Empty, category);
		}

		public List<IDevelopmentComponentError> QueryErrors(Guid microService)
		{
			return QueryErrors(microService, ErrorCategory.NotSet);
		}

		public List<IDevelopmentComponentError> QueryErrors()
		{
			return QueryErrors(Guid.Empty, ErrorCategory.NotSet);
		}

		public void ClearErrors(Guid component, Guid element, ErrorCategory category)
		{
			var u = Tenant.CreateUrl("DevelopmentErrors", "Clear");
			var e = new JObject
			{
				{"component", component },
				{"category", category.ToString() }
			};

			if (element != Guid.Empty)
				e.Add("element", element);

			Tenant.Post(u, e);
		}

		public void InsertErrors(Guid component, List<IDevelopmentError> errors)
		{
			var c = Tenant.GetService<IComponentService>().SelectComponent(component);

			if (c == null)
				throw new RuntimeException(SR.ErrComponentNotFound);

			var u = Tenant.CreateUrl("DevelopmentErrors", "Insert");
			var e = new JObject
			{
				{"microService", c.MicroService },
				{"component", c.Token }
			};

			var a = new JArray();
			e.Add("errors", a);

			foreach (var error in errors)
			{
				var je = new JObject
				{
					{"message", error.Message },
					{"severity", (int)error.Severity },
					{"category", (int)error.Category },
					{"code", error.Code }
				};

				if (error.Element != Guid.Empty)
					je.Add("element", error.Element);

				a.Add(je);
			}

			Tenant.Post(u, e);
		}

		public void RegisterAutoFix(IAutoFixProvider provider)
		{
			AutoFixProviders.Add(provider);
		}

		public List<IAutoFixProvider> QueryAutoFixProviders()
		{
			if (!AutoFixProvidersInitialized)
			{
				AutoFixProvidersInitialized = true;
				InitializeAutoFixProviders();
			}

			return AutoFixProviders.ToList();
		}

		private void InitializeAutoFixProviders()
		{
			lock (_autoFixProviders.Value)
			{
				var directory = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\\AutoFixes";

				if (!Directory.Exists(directory))
					return;

				var libraries = Directory.GetFiles(directory, "*.dll");

				foreach (var library in libraries)
					LoadAutoFix(library);
			}
		}

		private void LoadAutoFix(string path)
		{
			try
			{
				var asm = Assembly.LoadFrom(path);
				var types = asm.GetTypes();

				foreach (var type in types)
				{
					if (type.ImplementsInterface<IAutoFixProvider>())
					{
						var instance = type.CreateInstance<IAutoFixProvider>();

						if (instance != null)
							RegisterAutoFix(instance);
					}
				}
			}
			catch (Exception ex)
			{
				Tenant.GetService<ILoggingService>().Write(new LogEntry
				{
					Category = "Ide",
					EventId = IdeEvents.AutoFixInitialize,
					Level = System.Diagnostics.TraceLevel.Warning,
					Source = nameof(DesignerService),
					Message = ex.Message
				});
			}
		}

		public void AutoFix(string provider, Guid error)
		{
			var u = Tenant.CreateUrl("DevelopmentErrors", "AutoFix");
			var e = new JObject
			{
				{"provider", provider },
				{"error", error }
			};

			Tenant.Post(u, e);
		}

		private bool AutoFixProvidersInitialized { get; set; }
		private ConcurrentDictionary<string, IPropertyEditor> PropertyEditors => _propertyEditors.Value;
		private ConcurrentBag<IAutoFixProvider> AutoFixProviders => _autoFixProviders.Value;
	}
}
