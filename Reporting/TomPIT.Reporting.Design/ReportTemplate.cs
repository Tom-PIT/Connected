using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Reporting.Design.Storage;
using TomPIT.Reporting.UI;

namespace TomPIT.Reporting.Design
{
	public class ReportTemplate : MicroServiceTemplate
	{
		public override Guid Token { get { return new Guid("{206121ED-A805-4A32-B7CA-DC48F0F0AB92}"); } }
		public override string Name { get { return "Reports"; } }
		private static ConcurrentDictionary<string, IItemDescriptor> _items = null;

		static ReportTemplate()
		{
			_items = new ConcurrentDictionary<string, IItemDescriptor>(new Dictionary<string, IItemDescriptor>{
				{"Report", new ItemDescriptor("Report", "Report", typeof(Report)) { Category ="UI", Glyph="fal fa-browser", Ordinal = 101} },
				{"View", new ItemDescriptor("Report View", "View", typeof(ReportView)) { Category ="UI", Glyph="fal fa-browser", Ordinal = 102 } },
				{MasterView.ComponentCategory, new ItemDescriptor("Master view", MasterView.ComponentCategory, typeof(MasterView)) { Glyph = "fal fa-browser", Category = "UI" , Ordinal = 103} },
				{"Theme", new ItemDescriptor("Theme", "Theme", typeof(Theme)) { Category ="UI", Glyph="fal fa-pencil-paintbrush", Ordinal = 104} },

		});
		}

		public override List<IItemDescriptor> ProvideAddItems(IDomElement parent)
		{
			return _items.Values.ToList();
		}

		public override List<string> GetApplicationParts()
		{
			return new List<string>
			{
				"TomPIT.Reporting.Design.Views.dll"
			};
		}

		public override void Initialize(IApplicationBuilder app, IHostingEnvironment env)
		{
			DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension.RegisterExtensionGlobal(new ReportDesignerStorage());
		}
	}
}
