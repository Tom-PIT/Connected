namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class ObjectIdentity
	{
		public string Identity { get; set; }
		public int Seed { get; set; }
		public int Increment { get; set; }
		public bool NotForReplication { get; set; }
	}
}
