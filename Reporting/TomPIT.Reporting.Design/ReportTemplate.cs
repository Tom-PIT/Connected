using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using TomPIT.Design;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.Ide.ComponentModel;
using TomPIT.MicroServices.Reporting.Design.Storage;

namespace TomPIT.MicroServices.Reporting.Design
{
	public class ReportTemplate : MicroServiceTemplate
	{
		public override Guid Token { get { return new Guid("{206121ED-A805-4A32-B7CA-DC48F0F0AB92}"); } }
		public override string Name { get { return "Reports"; } }
		private static ConcurrentDictionary<string, IItemDescriptor> _items = null;

		static ReportTemplate()
		{
			_items = new ConcurrentDictionary<string, IItemDescriptor>(new Dictionary<string, IItemDescriptor>{
				{"Report", new ItemDescriptor("Report", "Report", typeof(Report)) { Category ="UI", Glyph="fal fa-browser", Ordinal = 242} }
		});
		}

		public override List<IItemDescriptor> ProvideGlobalAddItems(IDomElement parent)
		{
			return _items.Values.ToList();
		}

		public override List<IItemDescriptor> ProvideAddItems(IDomElement parent)
		{
			return _items.Values.ToList();
		}

		public override List<string> GetApplicationParts()
		{
			return new List<string>
			{
				"TomPIT.MicroServices.Reporting.Design.dll"
			};
		}

		public override void Initialize(IApplicationBuilder app, IWebHostEnvironment env)
		{
			DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension.RegisterExtensionGlobal(new ReportDesignerStorage());
		}

		public override TemplateKind Kind => TemplateKind.Plugin;

		public override List<IIdeResource> ProvideIdeResources()
		{
			return new List<IIdeResource>
			{
				new IdeResource
				{
					Type = IdeResourceType.Script,
					Path="~/Assets/reporting.design.min.js"
				},
				new IdeResource
				{
					Type = IdeResourceType.Stylesheet,
					Path="~/Assets/reporting.design.min.css"
				},
			};
		}
	}
}
