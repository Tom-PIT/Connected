using TomPIT.Environment;

namespace TomPIT.SysDb.Environment
{
	public class EnvironmentUnitBatchDescriptor
	{
		public IEnvironmentUnit Unit { get; set; }
		public string Name { get; set; }
		public IEnvironmentUnit Parent { get; set; }
		public int Ordinal { get; set; }
	}
}
