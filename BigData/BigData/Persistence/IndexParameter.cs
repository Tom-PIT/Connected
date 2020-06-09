using System;

namespace TomPIT.BigData.Persistence
{
	public abstract class IndexParameter
	{
		public string Name { get; set; }
		public Type ValueType { get; set; }
	}
}
