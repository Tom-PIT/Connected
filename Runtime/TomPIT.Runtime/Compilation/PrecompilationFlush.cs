using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;

namespace TomPIT.Compilation;
internal sealed class PrecompilationFlush : HostedService
{
	public PrecompilationFlush()
	{
		IntervalTimeout = TimeSpan.FromSeconds(15);
	}

	protected override async Task OnExecute(CancellationToken cancel)
	{
		Precompilation.Flush();

		await Task.CompletedTask;
	}
}
