using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TomPIT.ComponentModel;
using TomPIT.Ide.Collections;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Dom;
using TomPIT.MicroServices.Apis;
using TomPIT.MicroServices.Cdn;
using TomPIT.MicroServices.Data;
using TomPIT.MicroServices.Design.Media;
using TomPIT.MicroServices.Distributed;
using TomPIT.MicroServices.IoC;
using TomPIT.MicroServices.Messaging;
using TomPIT.MicroServices.Navigation;
using TomPIT.MicroServices.Resources;
using TomPIT.MicroServices.Search;
using TomPIT.MicroServices.UI;
using TomPIT.MicroServices.UI.Theming;

namespace TomPIT.MicroServices.Design
{
	public class MicroServicesTemplate : MicroServiceTemplate
	{
		public override Guid Token { get { return new Guid("{0FF75674-12F7-4EBB-8CF0-AD08A27319D4}"); } }
		public override string Name { get { return "Default"; } }
		private static ConcurrentDictionary<string, IItemDescriptor> _items = null;

		static MicroServicesTemplate()
		{
			_items = new ConcurrentDictionary<string, IItemDescriptor>(new Dictionary<string, IItemDescriptor>
			{
				{ ComponentCategories.View,            new ItemDescriptor("View",             ComponentCategories.View,              typeof(View))                                { Glyph = "fal fa-window-maximize text-primary",    Category = "UI",              Ordinal=0 } },
				{ ComponentCategories.MasterView,      new ItemDescriptor("Master view",      ComponentCategories.MasterView,        typeof(MasterView))                          { Glyph = "fal fa-window text-primary",             Category = "UI",              Ordinal=1 } },
				{ ComponentCategories.Partial,         new ItemDescriptor("Partial view",     ComponentCategories.Partial,           typeof(Partial))                             { Glyph = "fal fa-window-restore text-primary",     Category = "UI" ,             Ordinal=2} },
				{ ComponentCategories.Theme,           new ItemDescriptor("Theme",            ComponentCategories.Theme,             typeof(Theme))                               { Glyph = "fal fa-pencil-paintbrush text-primary",  Category = "UI" ,             Ordinal=3} },
				{ ComponentCategories.ScriptBundle,    new ItemDescriptor("Script bundle",    ComponentCategories.ScriptBundle,      typeof(ScriptBundle))                        { Glyph = "fal fa-file-code text-danger",                 Category = "UI",              Ordinal=4 } },
				{ ComponentCategories.SiteMap,         new ItemDescriptor("Site map",         ComponentCategories.SiteMap,           typeof(SiteMap))                             { Glyph = "fal fa-route",              Category = "UI",              Ordinal=5 } },
				{ ComponentCategories.Api,             new ItemDescriptor("Api",              ComponentCategories.Api,               typeof(Api))                                 { Glyph = "fal fa-broadcast-tower text-success",    Category = "Middleware" ,     Ordinal=100} },
				{ ComponentCategories.Script,          new ItemDescriptor("Script",           ComponentCategories.Script,            typeof(Scripting.Script))                    { Glyph = "fal fa-file-code text-success",          Category = "Middleware" ,     Ordinal=102} },
				{ ComponentCategories.IoCContainer,    new ItemDescriptor("IoC Container",    ComponentCategories.IoCContainer,      typeof(IoCContainerConfiguration))           { Glyph = "fal fa-file-code text-success",          Category = "Middleware" ,     Ordinal=103} },
				{ ComponentCategories.IoCEndpoint,     new ItemDescriptor("IoC Endpoints",     ComponentCategories.IoCEndpoint,       typeof(IoCEndpointConfiguration))            { Glyph = "fal fa-file-code text-success",          Category = "Middleware" ,     Ordinal=104} },
				{ ComponentCategories.EventBinder,     new ItemDescriptor("Event bindings",   ComponentCategories.EventBinder,       typeof(EventBindings))                       { Glyph = "fal fa-bullseye-pointer",   Category = "Middleware" ,     Ordinal=105} },
				{ ComponentCategories.DistributedEvent,new ItemDescriptor("Distributed events",ComponentCategories.DistributedEvent,  typeof(DistributedEvents))                    { Glyph = "fal fa-chart-network",      Category = "Distributed" ,    Ordinal=205} },
				{ ComponentCategories.HostedWorker,    new ItemDescriptor("Hosted worker",    ComponentCategories.HostedWorker,      typeof(HostedWorker))                        { Glyph = "fal fa-cog",                Category = "Distributed" ,    Ordinal=206} },
				{ ComponentCategories.Queue,           new ItemDescriptor("Queue middleware", ComponentCategories.Queue,             typeof(Queue))                               { Glyph = "fal fa-cog",                Category = "Distributed" ,    Ordinal=207} },
				{ ComponentCategories.Connection,      new ItemDescriptor("Connection",       ComponentCategories.Connection,        typeof(Connection))                          { Glyph = "fal fa-server",             Category = "Data" ,           Ordinal=302} },
				{ ComponentCategories.StringTable,     new ItemDescriptor("String table",     ComponentCategories.StringTable,       typeof(StringTable))                         { Glyph = "fal fa-font",               Category = "Resources" ,      Ordinal=400} } ,
				{ ComponentCategories.Media,           new ItemDescriptor("Media",            ComponentCategories.Media,             typeof(MediaResources))                      { Glyph = "fal fa-images",             Category = "Resources" ,      Ordinal=401} } ,
				{ ComponentCategories.EmbeddedAssembly,new ItemDescriptor("Embedded assembly",ComponentCategories.EmbeddedAssembly,  typeof(AssemblyEmbeddedResource))            { Glyph = "fal fa-file-code",          Category = "Resources" ,      Ordinal=402,      Value="Assembly"} } ,
				{ ComponentCategories.FileAssembly,    new ItemDescriptor("File assembly",    ComponentCategories.FileAssembly,      typeof(AssemblyFileSystemResource))          { Glyph = "fal fa-file-code",          Category = "Resources" ,      Ordinal=403,      Value="Assembly"} },
				{ ComponentCategories.Subscription,    new ItemDescriptor("Subscription",     ComponentCategories.Subscription,      typeof(Subscription))                        { Glyph = "fal fa-bell",               Category = "Cdn" ,            Ordinal=500} },
				{ ComponentCategories.MailTemplate,    new ItemDescriptor("Mail template",    ComponentCategories.MailTemplate,      typeof(MailTemplate))                        { Glyph = "fal fa-envelope text-primary",           Category = "Cdn" ,            Ordinal=501} },
				{ ComponentCategories.DataHub,         new ItemDescriptor("Data hub",         ComponentCategories.DataHub,           typeof(DataHub))                             { Glyph = "fal fa-bell",               Category = "Cdn" ,            Ordinal=502} },
				{ ComponentCategories.SearchCatalog,   new ItemDescriptor("Search catalog",   ComponentCategories.SearchCatalog,     typeof(SearchCatalog))                       { Glyph = "fal fa-search",             Category = "Search" ,         Ordinal=600} }
			});
		}

		public override List<IItemDescriptor> ProvideAddItems(IDomElement parent)
		{
			return _items.Values.ToList();
		}

		public override void RegisterRoutes(IRouteBuilder builder)
		{
			builder.MapRoute("sys/designers/application/media/{microService}/{component}", (t) =>
			{
				new MediaHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});
		}
	}
}
