using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using Xunit;

namespace TomPIT.Core.Tests.Distributed
{
	public class TimeoutTaskTests
	{
		[Fact]
		public void Stop_ReleasesRegistrationOnOuterToken()
		{
			using var outer = new CancellationTokenSource();

			var reference = StartAndStop(outer.Token);

			Collect();

			Assert.False(reference.IsAlive, "A stopped TimeoutTask must not be kept alive by the outer token.");
		}

		[Fact]
		public async Task StopAndDispose_AfterOuterCancel_EndsPingLoop()
		{
			using var outer = new CancellationTokenSource();
			var pings = 0;

			var timeout = new TimeoutTask(() =>
			{
				Interlocked.Increment(ref pings);
				return Task.CompletedTask;
			}, TimeSpan.FromMilliseconds(20), outer.Token);

			timeout.Start();
			await Task.Delay(100);

			outer.Cancel();
			timeout.Stop();
			timeout.Dispose();

			var count = Volatile.Read(ref pings);

			await Task.Delay(150);

			Assert.Equal(count, Volatile.Read(ref pings));
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static WeakReference StartAndStop(CancellationToken token)
		{
			var timeout = new TimeoutTask(() => Task.CompletedTask, TimeSpan.FromMinutes(1), token);

			timeout.Start();
			timeout.Stop();

			return new WeakReference(timeout);
		}

		internal static void Collect()
		{
			for (var i = 0; i < 3; i++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}
	}
}
