using System.Threading.Tasks;
using TomPIT.Caching;

namespace TomPIT.Sys.Caching
{
	public abstract class PersistentRepository<T, K> : SynchronizedRepository<T, K> where T : class
	{
		private bool _dirty = false;
		public PersistentRepository(IMemoryCache container, string key) : base(container, key)
		{
		}

		protected bool IsDirty => _dirty;

		protected void Dirty()
		{
			_dirty = true;
		}

		protected void Clean()
		{
			_dirty = false;
		}

		public virtual async Task Flush()
		{
			Initialize();

			if (!IsDirty)
				return;

			Clean();

			await OnFlushing();
		}

		protected virtual async Task OnFlushing()
		{
			await Task.CompletedTask;
		}
	}
}
