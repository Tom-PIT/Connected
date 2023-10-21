using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace TomPIT.Runtime;
public abstract class Startup : IStartup
{
	public virtual void Configure(IApplicationBuilder app)
	{
	}

	public virtual void ConfigureServices(IServiceCollection services)
	{
	}

	public virtual async Task Initialize()
	{
		await Task.CompletedTask;
	}

	public virtual async Task Start()
	{
		await Task.CompletedTask;
	}
}
