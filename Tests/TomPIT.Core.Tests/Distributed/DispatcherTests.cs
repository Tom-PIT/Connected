using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using TomPIT.Distributed;
using Xunit;

namespace TomPIT.Core.Tests.Distributed
{
	public class DispatcherTests
	{
		[Fact]
		public void DispatcherJob_Dispose_ReleasesRegistrationOnOwnerToken()
		{
			using var owner = new TestDispatcher();
			using var cancel = new CancellationTokenSource();

			var reference = CreateAndDisposeJob(owner, cancel.Token);

			TimeoutTaskTests.Collect();

			Assert.False(reference.IsAlive, "A disposed job must not be kept alive by the dispatcher's token.");
		}

		[Fact]
		public void QueuedDispatcher_CreateWorker_DelegatesToOwner()
		{
			using var owner = new TestDispatcher();
			using var queued = new QueuedDispatcher<string>(owner, "queue");
			using var cancel = new CancellationTokenSource();

			using var worker = queued.CreateWorker(queued, cancel.Token);

			Assert.IsType<TestJob>(worker);
		}

		[Fact]
		public void Dispatcher_CompletedQueuedDispatcherUnderGeneratedKey_IsRemoved()
		{
			using var owner = new TestDispatcher();
			var queued = new QueuedDispatcher<string>(owner, "queue");
			/*
			 * A dispatcher that received an item while being removed is re-added under a generated key.
			 */
			owner.QueuedDispatchers.TryAdd($"queue_{Guid.NewGuid()}", queued);

			typeof(Dispatcher<string>)
				.GetMethod("OnQueuedCompleted", BindingFlags.Instance | BindingFlags.NonPublic)
				.Invoke(owner, new object[] { queued, EventArgs.Empty });

			Assert.Empty(owner.QueuedDispatchers);
			Assert.True(queued.Disposed);
		}

		[Fact]
		public void Dispatcher_ProcessesAllItems()
		{
			using var owner = new TestDispatcher();

			for (var i = 0; i < 50; i++)
				owner.Enqueue(i.ToString());

			Assert.True(SpinWait.SpinUntil(() => owner.Processed == 50, TimeSpan.FromSeconds(10)));
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static WeakReference CreateAndDisposeJob(IDispatcher<string> owner, CancellationToken token)
		{
			var job = new TestJob(owner, token, null);

			job.Dispose();

			return new WeakReference(job);
		}

		private sealed class TestDispatcher : Dispatcher<string>
		{
			private int _processed;

			public TestDispatcher() : base(4)
			{
			}

			public int Processed => Volatile.Read(ref _processed);

			public override DispatcherJob<string> CreateWorker(IDispatcher<string> owner, CancellationToken cancel)
			{
				return new TestJob(owner, cancel, () => Interlocked.Increment(ref _processed));
			}
		}

		private sealed class TestJob : DispatcherJob<string>
		{
			private readonly Action _processed;

			public TestJob(IDispatcher<string> owner, CancellationToken cancel, Action processed) : base(owner, cancel)
			{
				_processed = processed;
			}

			protected override void DoWork(string item)
			{
				_processed?.Invoke();
			}

			protected override void OnError(string item, Exception ex)
			{
			}
		}
	}
}
