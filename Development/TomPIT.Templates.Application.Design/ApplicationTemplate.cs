using System;
using System.Collections.Generic;
using TomPIT.Application.Apis;
using TomPIT.Application.Data;
using TomPIT.Application.Events;
using TomPIT.Application.Resources;
using TomPIT.Application.UI;
using TomPIT.Application.Workers;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Application
{
	public class ApplicationTemplate : MicroServiceTemplate
	{
		public override Guid Token { get { return new Guid("{0FF75674-12F7-4EBB-8CF0-AD08A27319D4}"); } }
		public override string Name { get { return "Business Application"; } }

		public override List<IItemDescriptor> ProvideAddItems(IDomElement parent)
		{
			var r = new List<IItemDescriptor>();


			//if (string.Compare(category, Area.ComponentCategory, true) == 0)
			//	r.Add(new ItemDescriptor("Area", "Area", typeof(Area)));
			//else if (string.Compare(category, View.ComponentCategory, true) == 0)
			r.Add(new ItemDescriptor("View", "View", typeof(View)) { Glyph = "fal fa-browser", Category = "UI", Value = "View" });
			r.Add(new ItemDescriptor("Master view", "MasterView", typeof(MasterView)) { Glyph = "fal fa-browser", Category = "UI", Value = "MasterView" });
			r.Add(new ItemDescriptor("Partial view", "PartialView", typeof(Partial)) { Glyph = "fal fa-browser", Category = "UI", Value = "PartialView" });
			r.Add(new ItemDescriptor("Theme", "Theme", typeof(Theme)) { Glyph = "fal fa-pencil-paintbrush", Category = "UI", Value = "Theme" });
			r.Add(new ItemDescriptor("Script bundle", ScriptBundle.ComponentCategory, typeof(ScriptBundle)) { Glyph = "fab fa-js", Category = "UI", Value = "Bundle" });

			r.Add(new ItemDescriptor("Api", "Api", typeof(Api)) { Glyph = "fal fa-broadcast-tower", Category = "Model", Value = "Api" });
			r.Add(new ItemDescriptor("Library", "Library", typeof(Library)) { Glyph = "fal fa-file-code", Category = "Model", Value = "Library" });
			r.Add(new ItemDescriptor("Event handler", Events.EventHandler.ComponentCategory, typeof(Events.EventHandler)) { Glyph = "fal fa-bullseye-pointer", Category = "Model", Value = "EventHandler" });
			r.Add(new ItemDescriptor("Distributed event", DistributedEvent.ComponentCategory, typeof(DistributedEvent)) { Glyph = "fal fa-chart-network", Category = "Model", Value = "Event" });
			r.Add(new ItemDescriptor("Hosted worker", HostedWorker.ComponentCategory, typeof(HostedWorker)) { Glyph = "fal fa-cog", Category = "Model", Value = "Worker" });

			r.Add(new ItemDescriptor("Data source", "DataSource", typeof(DataSource)) { Glyph = "fal fa-database", Category = "Data", Value = "Data" });
			r.Add(new ItemDescriptor("Transaction", "Transaction", typeof(Transaction)) { Glyph = "fal fa-exchange-alt", Category = "Data", Value = "Transaction" });
			//r.Add(new ItemDescriptor("Data management", DataManagement.ComponentCategory, typeof(DataManagement)) { Glyph = "fal fa-exchange-alt", Category = "Data", Value = "DataManagement" });
			r.Add(new ItemDescriptor("Connection", "Connection", typeof(Connection)) { Glyph = "fal fa-server", Category = "Data", Value = "Connection" });

			r.Add(new ItemDescriptor("Upload assembly", "Upload", typeof(AssemblyUploadResource)) { Glyph = "fal fa-file-code", Category = "Resources", Value = "Upload" });
			r.Add(new ItemDescriptor("File assembly", "File", typeof(AssemblyFileSystemResource)) { Glyph = "fal fa-file-code", Category = "Resources", Value = "File" });

			return r;
		}

		public override List<IDomElement> QueryDomRoot(IEnvironment environment, IDomElement parent, Guid microService)
		{
			return new List<IDomElement>
			{
				//new FeaturesElement(environment, parent),
				//new ResourcesElement(environment, parent),
				//new ComponentElement(environment, parent, CreateReferences(environment, microService)),
			};
		}
	}
}
