using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Application.Apis;
using TomPIT.Application.Cdn;
using TomPIT.Application.Data;
using TomPIT.Application.Events;
using TomPIT.Application.Features;
using TomPIT.Application.QA;
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
				{View.ComponentCategory,  new ItemDescriptor("View", View.ComponentCategory, typeof(View)) { Glyph = "fal fa-browser", Category = "UI", Ordinal=0 } },
				{MasterView.ComponentCategory, new ItemDescriptor("Master view", MasterView.ComponentCategory, typeof(MasterView)) { Glyph = "fal fa-browser", Category = "UI", Ordinal=1 } },
				{Partial.ComponentCategory, new ItemDescriptor("Partial view", Partial.ComponentCategory, typeof(Partial)) { Glyph = "fal fa-browser", Category = "UI" , Ordinal=2} },
				{Theme.ComponentCategory, new ItemDescriptor("Theme", Theme.ComponentCategory, typeof(Theme)) { Glyph = "fal fa-pencil-paintbrush", Category = "UI" , Ordinal=3} },
				{ScriptBundle.ComponentCategory,new ItemDescriptor("Script bundle", ScriptBundle.ComponentCategory, typeof(ScriptBundle)) { Glyph = "fab fa-js", Category = "UI", Ordinal=4 } },
				{Api.ComponentCategory, new ItemDescriptor("Api", Api.ComponentCategory, typeof(Api)) { Glyph = "fal fa-broadcast-tower", Category = "Model" , Ordinal=100} },
					 {Script.ComponentCategory, new ItemDescriptor("Script", Script.ComponentCategory, typeof(Script)) { Glyph = "fal fa-file-code", Category = "Model" , Ordinal=102} },
                { Library.ComponentCategory, new ItemDescriptor("Library", Library.ComponentCategory, typeof(Library)) { Glyph = "fal fa-file-code", Category = "Model" , Ordinal=103} },
				{Events.EventHandler.ComponentCategory, new ItemDescriptor("Event handler", Events.EventHandler.ComponentCategory, typeof(Events.EventHandler)) { Glyph = "fal fa-bullseye-pointer", Category = "Model" , Ordinal=104} },
				{DistributedEvent.ComponentCategory, new ItemDescriptor("Distributed event", DistributedEvent.ComponentCategory, typeof(DistributedEvent)) { Glyph = "fal fa-chart-network", Category = "Model" , Ordinal=105} },
				{HostedWorker.ComponentCategory, new ItemDescriptor("Hosted worker", HostedWorker.ComponentCategory, typeof(HostedWorker)) { Glyph = "fal fa-cog", Category = "Model" , Ordinal=106} },
				{QueueWorker.ComponentCategory, new ItemDescriptor("Queue worker", QueueWorker.ComponentCategory, typeof(QueueWorker)) { Glyph = "fal fa-cog", Category = "Model" , Ordinal=107} },
				{DataSource.ComponentCategory, new ItemDescriptor("Data source", "DataSource", typeof(DataSource)) { Glyph = "fal fa-database", Category = "Data" , Ordinal=200} },
				{Transaction.ComponentCategory, new ItemDescriptor("Transaction", "Transaction", typeof(Transaction)) { Glyph = "fal fa-exchange-alt", Category = "Data" , Ordinal=201} },
				//r.Add(new ItemDescriptor("Data management", DataManagement.ComponentCategory, typeof(DataManagement)) { Glyph = "fal fa-exchange-alt", Category = "Data", Value = "DataManagement" });
				{Connection.ComponentCategory, new ItemDescriptor("Connection", "Connection", typeof(Connection)) { Glyph = "fal fa-server", Category = "Data" , Ordinal=202} },
				{"FeatureSet", new ItemDescriptor("Feature set", "FeatureSet", typeof(FeatureSet)) { Glyph = "fal fa-function", Category = "Configuration" , Ordinal=250} },
				{"Strings", new ItemDescriptor("String table", "StringTable", typeof(StringTable)) { Glyph = "fal fa-font", Category = "Resources" , Ordinal=300} } ,
				{"Media", new ItemDescriptor("Media", "Media", typeof(MediaResources)) { Glyph = "fal fa-images", Category = "Resources" , Ordinal=301} } ,
				{ "Embedded", new ItemDescriptor("Embedded assembly", "Embedded Assembly", typeof(AssemblyEmbeddedResource)) { Glyph = "fal fa-file-code", Category = "Resources" , Ordinal=303, Value="Assembly"} } ,
				{"File", new ItemDescriptor("File assembly", "File Assembly", typeof(AssemblyFileSystemResource)) { Glyph = "fal fa-file-code", Category = "Resources" , Ordinal=304, Value="Assembly"} },
				{"Subscription", new ItemDescriptor("Subscription", "Subscription", typeof(Subscription)) { Glyph = "fal fa-bell", Category = "Cdn" , Ordinal=400} },
				{"MailTemplate", new ItemDescriptor("Mail template", "MailTemplate", typeof(MailTemplate)) { Glyph = "fal fa-envelope", Category = "Cdn" , Ordinal=401} },
				{"TestSuite", new ItemDescriptor("Test suite", "TestSuite", typeof(TestSuite)) { Glyph = "fal fa-stethoscope", Category = "QA" , Ordinal=500} }
			});
		}

		public override List<IItemDescriptor> ProvideAddItems(IDomElement parent)
		{
			return _items.Values.ToList();
		}
	}
}
