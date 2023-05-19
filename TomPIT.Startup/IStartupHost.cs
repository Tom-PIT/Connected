using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace TomPIT.Startup;

public interface IStartupHost
{
	event EventHandler<IServiceCollection> ConfiguringServices;
	event EventHandler<Tuple<IApplicationBuilder, IWebHostEnvironment>> Configuring;

	event EventHandler Booting;
	event EventHandler SysProxyCreated;
	event EventHandler<HubOptions> ConfiguringSignalR;
	event EventHandler<IEndpointRouteBuilder> ConfiguringRouting;
	event EventHandler<IRouteBuilder> ConfiguringMvcRouting;
	event EventHandler<MvcOptions> ConfiguringMvc;
	event EventHandler<ApplicationPartsArgs> ConfiguringApplicationParts;
	event EventHandler<List<Assembly>> ConfigureEmbeddedStaticResources;

	event EventHandler<IMvcBuilder> MvcConfigured;
}
