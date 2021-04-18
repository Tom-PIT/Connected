using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Runtime;

namespace TomPIT.Data
{
	public abstract class DependencyGraph<K, T> : TenantObject
	{
		private List<DependencyGraphEntry> _items;
		private ConcurrentDictionary<K, DependencyGraphEntry> _index;
		private SingletonProcessor<int> _processor = new SingletonProcessor<int>();

		protected DependencyGraph(ITenant tenant) : base(tenant)
		{
		}

		private class DependencyGraphEntry
		{
			private ConcurrentDictionary<K, DependencyGraphEntry> _references;
			private ConcurrentDictionary<K, DependencyGraphEntry> _referencedBy;
			public T Item { get; set; }

			public ConcurrentDictionary<K, DependencyGraphEntry> References => _references ??= new ConcurrentDictionary<K, DependencyGraphEntry>();
			public ConcurrentDictionary<K, DependencyGraphEntry> ReferencedBy => _referencedBy ??= new ConcurrentDictionary<K, DependencyGraphEntry>();
		}

		public ImmutableArray<T> References(K key, bool recursive)
		{
			Initialize();

			var result = new Dictionary<K, T>();

			References(key, result, recursive);

			return result.Values.ToImmutableArray();
		}

		private void References(K key, Dictionary<K, T> items, bool recursive)
		{
			if (!Index.TryGetValue(key, out DependencyGraphEntry entry) || !entry.References.Any())
				return;

			foreach (var reference in entry.References)
			{
				if (!items.ContainsKey(reference.Key))
					items.Add(reference.Key, reference.Value.Item);
			}

			if (!recursive)
				return;

			foreach (var reference in entry.References)
				References(reference.Key, items, recursive);
		}

		public ImmutableArray<T> ReferencedBy(K key, bool recursive)
		{
			Initialize();

			var result = new Dictionary<K, T>();

			ReferencedBy(key, result, recursive);

			return result.Values.ToImmutableArray();
		}

		private void ReferencedBy(K key, Dictionary<K, T> items, bool recursive)
		{
			if (!Index.TryGetValue(key, out DependencyGraphEntry entry) || !entry.References.Any())
				return;

			foreach (var reference in entry.ReferencedBy)
			{
				if (!items.ContainsKey(reference.Key))
					items.Add(reference.Key, reference.Value.Item);
			}

			if (!recursive)
				return;

			foreach (var reference in entry.ReferencedBy)
				ReferencedBy(reference.Key, items, recursive);
		}

		public void Remove(K key)
		{
			if (!Index.TryGetValue(key, out DependencyGraphEntry entry))
				return;

			foreach(var reference in entry.ReferencedBy)
				reference.Value.References.TryRemove(key, out _);

			Index.TryRemove(key, out _);
		}

		protected void Set(K key, T item, ImmutableDictionary<K, T> references)
		{
			var entry = Ensure(key, item);

			entry.References.Clear();

			foreach(var reference in references)
			{
				var refEntry = Ensure(reference.Key, reference.Value);

				entry.References.TryAdd(reference.Key, refEntry);
				refEntry.ReferencedBy.TryAdd(key, entry);
			}
		}

		private DependencyGraphEntry Ensure(K key, T item)
		{
			if (Index.TryGetValue(key, out DependencyGraphEntry entry))
				return entry;

			entry = new DependencyGraphEntry
			{
				Item = item
			};

			if (!Index.TryAdd(key, entry) && !Index.TryGetValue(key, out entry))
				throw new RuntimeException(SR.ErrDependencyGraph);

			return entry;
		}

		private void Initialize()
		{
			if (Processor.IsInitialized)
				return;

			Processor.Start(0,
				() =>
				{
					OnInitialize();
				});
		}

		protected virtual void OnInitialize()
		{

		}

		private List<DependencyGraphEntry> Items => _items ??= new List<DependencyGraphEntry>();
		private ConcurrentDictionary<K, DependencyGraphEntry> Index => _index ??= new ConcurrentDictionary<K, DependencyGraphEntry>();

		private SingletonProcessor<int> Processor => _processor;
	}
}