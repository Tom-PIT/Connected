using System;

namespace TomPIT.Data
{
	public class ConnectionString : IConnectionString
	{
		public Guid DataProvider { get; set; }
		public string Value { get; set; }
	}
}
