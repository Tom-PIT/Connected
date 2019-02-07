using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Application.Apis;
using TomPIT.Application.Data;
using TomPIT.Application.Events;
using TomPIT.Application.Resources;
using TomPIT.Application.UI;
using TomPIT.Application.Workers;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application
{
	public class ApplicationTemplate : MicroServiceTemplate
	{
		public override Guid Token { get { return new Guid("{0FF75674-12F7-4EBB-8CF0-AD08A27319D4}"); } }
		public override string Name { get { return "Business Application"; } }
		private static ConcurrentDictionary<string, IItemDescriptor> _items = null;

		static ApplicationTemplate()
		{
			_items = new ConcurrentDictionary<string, IItemDescriptor>(new Dictionary<string, IItemDescriptor>{
				{View.ComponentCategory,  new ItemDescriptor("View", View.ComponentCategory, typeof(View)) { Glyph = "fal fa-browser", Category = "UI" } },
				{MasterView.ComponentCategory, new ItemDescriptor("Master view", MasterView.ComponentCategory, typeof(MasterView)) { Glyph = "fal fa-browser", Category = "UI" } },
				{Partial.ComponentCategory, new ItemDescriptor("Partial view", Partial.ComponentCategory, typeof(Partial)) { Glyph = "fal fa-browser", Category = "UI" } },
				{Theme.ComponentCategory, new ItemDescriptor("Theme", Theme.ComponentCategory, typeof(Theme)) { Glyph = "fal fa-pencil-paintbrush", Category = "UI" } },
				{ScriptBundle.ComponentCategory,new ItemDescriptor("Script bundle", ScriptBundle.ComponentCategory, typeof(ScriptBundle)) { Glyph = "fab fa-js", Category = "UI" } },
				{Api.ComponentCategory, new ItemDescriptor("Api", Api.ComponentCategory, typeof(Api)) { Glyph = "fal fa-broadcast-tower", Category = "Model" } },
				{Library.ComponentCategory, new ItemDescriptor("Library", Library.ComponentCategory, typeof(Library)) { Glyph = "fal fa-file-code", Category = "Model" } },
				{Events.EventHandler.ComponentCategory, new ItemDescriptor("Event handler", Events.EventHandler.ComponentCategory, typeof(Events.EventHandler)) { Glyph = "fal fa-bullseye-pointer", Category = "Model" } },
				{DistributedEvent.ComponentCategory, new ItemDescriptor("Distributed event", DistributedEvent.ComponentCategory, typeof(DistributedEvent)) { Glyph = "fal fa-chart-network", Category = "Model" } },
				{HostedWorker.ComponentCategory, new ItemDescriptor("Hosted worker", HostedWorker.ComponentCategory, typeof(HostedWorker)) { Glyph = "fal fa-cog", Category = "Model" } },
				{DataSource.ComponentCategory, new ItemDescriptor("Data source", "DataSource", typeof(DataSource)) { Glyph = "fal fa-database", Category = "Data" } },
				{Transaction.ComponentCategory, new ItemDescriptor("Transaction", "Transaction", typeof(Transaction)) { Glyph = "fal fa-exchange-alt", Category = "Data" } },
				//r.Add(new ItemDescriptor("Data management", DataManagement.ComponentCategory, typeof(DataManagement)) { Glyph = "fal fa-exchange-alt", Category = "Data", Value = "DataManagement" });
				{Connection.ComponentCategory, new ItemDescriptor("Connection", "Connection", typeof(Connection)) { Glyph = "fal fa-server", Category = "Data" } },
				{"Upload", new ItemDescriptor("Upload assembly", "Upload", typeof(AssemblyUploadResource)) { Glyph = "fal fa-file-code", Category = "Resources" } } ,
				{"File", new ItemDescriptor("File assembly", "File", typeof(AssemblyFileSystemResource)) { Glyph = "fal fa-file-code", Category = "Resources" } }
			});
		}

		public override List<IItemDescriptor> ProvideAddItems(IDomElement parent)
		{
			return _items.Values.ToList();
		}
	}
}
