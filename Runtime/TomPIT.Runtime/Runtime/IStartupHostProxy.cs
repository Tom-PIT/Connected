using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TomPIT.Startup;

namespace TomPIT.Runtime;
public interface IStartupHostProxy : IStartupHost
{
	void ConfigureServices(IServiceCollection services);
	void Configure(IApplicationBuilder app, IWebHostEnvironment env);
}
