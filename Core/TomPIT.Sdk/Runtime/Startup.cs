using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.Runtime;
public abstract class Startup : IStartup
{
	public Startup()
	{
		HasRecompiled = Tenant.GetService<IRuntimeService>().RecompiledMicroServices.FirstOrDefault(f => f == GetType().Assembly) is not null;
	}

	public bool HasRecompiled { get; }
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		OnConfigure(app, env);
	}

	protected virtual void OnConfigure(IApplicationBuilder app, IWebHostEnvironment env)
	{
	}

	public void ConfigureServices(IServiceCollection services)
	{
		OnConfigureServices(services);
	}

	protected virtual void OnConfigureServices(IServiceCollection services)
	{

	}

	public async Task Initialize()
	{
		await OnInitialize();
	}

	protected virtual async Task OnInitialize()
	{
		await Task.CompletedTask;
	}

	public async Task Start()
	{
		await OnStart();
	}

	protected virtual async Task OnStart()
	{
		await Task.CompletedTask;
	}
}
