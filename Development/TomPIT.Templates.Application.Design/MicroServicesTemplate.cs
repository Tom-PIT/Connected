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
using TomPIT.MicroServices.Configuration;
using TomPIT.MicroServices.Data;
using TomPIT.MicroServices.Deployment;
using TomPIT.MicroServices.Design.Media;
using TomPIT.MicroServices.Distributed;
using TomPIT.MicroServices.IoC;
using TomPIT.MicroServices.Management;
using TomPIT.MicroServices.Messaging;
using TomPIT.MicroServices.Navigation;
using TomPIT.MicroServices.Quality;
using TomPIT.MicroServices.Resources;
using TomPIT.MicroServices.Runtime;
using TomPIT.MicroServices.Search;
using TomPIT.MicroServices.Security;
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
				{ ComponentCategories.Connection,      new ItemDescriptor("Connection",       ComponentCategories.Connection,        typeof(Connection))                          { Glyph = "fal fa-server",             Category = "Data" ,           Ordinal=10} },
				{ ComponentCategories.Model,             new ItemDescriptor("Model",              ComponentCategories.Model,               typeof(Model))                                 { Glyph = "fal fa-file-code",    Category = "Data" ,     Ordinal=20} },

				{ ComponentCategories.Api,             new ItemDescriptor("Api",              ComponentCategories.Api,               typeof(Api))                                 { Glyph = "fal fa-broadcast-tower",    Category = "Middleware" ,     Ordinal=110} },
				{ ComponentCategories.Script,          new ItemDescriptor("Script",           ComponentCategories.Script,            typeof(Scripting.Script))                    { Glyph = "fal fa-file-code",          Category = "Middleware" ,     Ordinal=120} },
				{ ComponentCategories.DistributedEvent,new ItemDescriptor("Distributed events",ComponentCategories.DistributedEvent,  typeof(DistributedEvents))                    { Glyph = "fal fa-chart-network",      Category = "Middleware" ,    Ordinal=130} },
				{ ComponentCategories.HostedWorker,    new ItemDescriptor("Hosted worker",    ComponentCategories.HostedWorker,      typeof(HostedWorker))                        { Glyph = "fal fa-cog",                Category = "Middleware" ,    Ordinal=140} },
				{ ComponentCategories.Queue,           new ItemDescriptor("Queue middleware", ComponentCategories.Queue,             typeof(Queue))                               { Glyph = "fal fa-cog",                Category = "Middleware" ,    Ordinal=150} },
				{ ComponentCategories.EventBinder,     new ItemDescriptor("Event bindings",   ComponentCategories.EventBinder,       typeof(EventBindings))                       { Glyph = "fal fa-bullseye-pointer",   Category = "Middleware" ,     Ordinal=160} },
				{ ComponentCategories.SearchCatalog,   new ItemDescriptor("Search catalog",   ComponentCategories.SearchCatalog,     typeof(SearchCatalog))                       { Glyph = "fal fa-search",             Category = "Middleware" ,         Ordinal=170} },

				{ ComponentCategories.UIDependencyInjection,     new ItemDescriptor("UI Dependency Injection",     ComponentCategories.UIDependencyInjection,       typeof(UIDependencyInjectionConfiguration))            { Glyph = "fal fa-file-code",          Category = "Middleware" ,     Ordinal=180} },
				{ ComponentCategories.IoCContainer,    new ItemDescriptor("IoC Container",    ComponentCategories.IoCContainer,      typeof(IoCContainerConfiguration))           { Glyph = "fal fa-file-code",          Category = "Middleware" ,     Ordinal=190} },
				{ ComponentCategories.IoCEndpoint,     new ItemDescriptor("IoC Endpoints",     ComponentCategories.IoCEndpoint,       typeof(IoCEndpointConfiguration))            { Glyph = "fal fa-file-code",          Category = "Middleware" ,     Ordinal=191} },
				{ ComponentCategories.DependencyInjection,     new ItemDescriptor("Dependency Injection",     ComponentCategories.DependencyInjection,       typeof(DependencyInjectionConfiguration))            { Glyph = "fal fa-file-code",          Category = "Middleware" ,     Ordinal=192} },
				{ ComponentCategories.PermissionDescriptor,     new ItemDescriptor("Permission Descriptor",     ComponentCategories.PermissionDescriptor,       typeof(PermissionDescriptor))            { Glyph = "fal fa-shield",          Category = "Middleware" ,     Ordinal=193} },

				{ ComponentCategories.View,            new ItemDescriptor("View",             ComponentCategories.View,              typeof(View))                                { Glyph = "fal fa-window-maximize",    Category = "UI",              Ordinal=210 } },
				{ ComponentCategories.Partial,         new ItemDescriptor("Partial view",     ComponentCategories.Partial,           typeof(Partial))                             { Glyph = "fal fa-window-restore",     Category = "UI" ,             Ordinal=220} },
				{ ComponentCategories.ScriptBundle,    new ItemDescriptor("Script bundle",    ComponentCategories.ScriptBundle,      typeof(ScriptBundle))                        { Glyph = "fal fa-file-code",                 Category = "UI",              Ordinal=230 } },
				{ ComponentCategories.MasterView,      new ItemDescriptor("Master view",      ComponentCategories.MasterView,        typeof(MasterView))                          { Glyph = "fal fa-window",             Category = "UI",              Ordinal=240 } },
				{ ComponentCategories.Theme,           new ItemDescriptor("Theme",            ComponentCategories.Theme,             typeof(Theme))                               { Glyph = "fal fa-pencil-paintbrush",  Category = "UI" ,             Ordinal=250} },
				{ ComponentCategories.SiteMap,         new ItemDescriptor("Site map",         ComponentCategories.SiteMap,           typeof(SiteMap))                             { Glyph = "fal fa-route",              Category = "UI",              Ordinal=260 } },

				{ ComponentCategories.MailTemplate,    new ItemDescriptor("Mail template",    ComponentCategories.MailTemplate,      typeof(MailTemplate))                        { Glyph = "fal fa-envelope",           Category = "Distribution" ,            Ordinal=310} },
				{ ComponentCategories.Subscription,    new ItemDescriptor("Subscription",     ComponentCategories.Subscription,      typeof(Subscription))                        { Glyph = "fal fa-bell",               Category = "Distribution" ,            Ordinal=320} },
				{ ComponentCategories.Inbox,    new ItemDescriptor("Inbox",    ComponentCategories.Inbox,      typeof(Inbox))                        { Glyph = "fal fa-envelope",           Category = "Distribution" ,            Ordinal=325} },
				{ ComponentCategories.SmtpConnection,  new ItemDescriptor("SMTP Connection",  ComponentCategories.SmtpConnection,    typeof(SmtpConnection))                      { Glyph = "fal fa-envelope",               Category = "Distribution" ,            Ordinal=330} },

				{ ComponentCategories.StringTable,     new ItemDescriptor("String table",     ComponentCategories.StringTable,       typeof(StringTable))                         { Glyph = "fal fa-font",               Category = "Resources" ,      Ordinal=400} } ,
				{ ComponentCategories.Media,           new ItemDescriptor("Media",            ComponentCategories.Media,             typeof(MediaResources))                      { Glyph = "fal fa-images",             Category = "Resources" ,      Ordinal=401} } ,
				{ ComponentCategories.EmbeddedAssembly,new ItemDescriptor("Embedded assembly",ComponentCategories.EmbeddedAssembly,  typeof(AssemblyEmbeddedResource))            { Glyph = "fal fa-file-code",          Category = "Resources" ,      Ordinal=402 }  } ,
				{ ComponentCategories.FileAssembly,    new ItemDescriptor("File assembly",    ComponentCategories.FileAssembly,      typeof(AssemblyFileSystemResource))          { Glyph = "fal fa-file-code",          Category = "Resources" ,      Ordinal=403} },
				{ ComponentCategories.Text,    new ItemDescriptor("Text",    ComponentCategories.Text,      typeof(Resources.Text))          { Glyph = "fal fa-file-alt",          Category = "Resources" ,      Ordinal=404} },

				{ ComponentCategories.MicroServiceInfo,     new ItemDescriptor("Microservice info",   ComponentCategories.MicroServiceInfo,       typeof(MicroServiceInfoConfiguration))                       { Glyph = "fal fa-cogs",   Category = "Infrastructure" ,     Ordinal=504} },
				{ ComponentCategories.Settings,     new ItemDescriptor("Settings",   ComponentCategories.Settings,       typeof(SettingsConfiguration))                       { Glyph = "fal fa-cogs",   Category = "Infrastructure" ,     Ordinal=505} },
				{ ComponentCategories.Installer,       new ItemDescriptor("Installer",        ComponentCategories.Installer,         typeof(Installer))                           { Glyph = "fal fa-inbox-in",           Category = "Infrastructure" ,      Ordinal=510} },
				{ ComponentCategories.Runtime,       new ItemDescriptor("Runtime",        ComponentCategories.Runtime,         typeof(RuntimeConfiguration))                           { Glyph = "fal fa-cogs",           Category = "Infrastructure" ,      Ordinal=520} },
				{ ComponentCategories.Management,     new ItemDescriptor("Management",   ComponentCategories.Management,       typeof(ManagementConfiguration))                       { Glyph = "fal fa-cogs",   Category = "Infrastructure" ,     Ordinal=530} },

				{ ComponentCategories.UnitTest,     new ItemDescriptor("Unit Test",   ComponentCategories.UnitTest,       typeof(UnitTest))                       { Glyph = "fal fa-file-code",   Category = "Quality" ,     Ordinal=610} },
			});
		}

		public override List<IItemDescriptor> ProvideAddItems(IDomElement parent)
		{
			return _items.Values.ToList();
		}

		public override void RegisterRoutes(IEndpointRouteBuilder builder)
		{
			builder.Map("sys/designers/application/media/{microService}/{component}", (t) =>
			{
				new MediaHandler().ProcessRequest(t);

				return Task.CompletedTask;
			});
		}
	}
}
