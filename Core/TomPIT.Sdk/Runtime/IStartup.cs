using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace TomPIT.Runtime;
public interface IStartup
{
	void ConfigureServices(IServiceCollection services);
	void Configure(IApplicationBuilder app);
	Task Initialize();
	Task Start();
}
