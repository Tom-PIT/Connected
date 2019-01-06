namespace TomPIT.Services
{
	internal class ServiceInstance
	{
		private object _syncRoot = new object();
		public object Value { get; set; }
		public object SyncRoot { get { return _syncRoot; } }
	}
}
