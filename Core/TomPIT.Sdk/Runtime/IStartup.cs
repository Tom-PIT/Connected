using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace TomPIT.Runtime;
public interface IStartup
{
	void ConfigureServices(IServiceCollection services);
	void Configure(IApplicationBuilder app, IWebHostEnvironment env);
	Task Initialize();
	Task Start();

	bool HasRecompiled { get; }
}
