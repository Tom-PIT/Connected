using System.Collections.Generic;

namespace TomPIT.Deployment.Database
{
	public interface IDatabase
	{
		List<ITable> Tables { get; }
		List<IView> Views { get; }
		List<IRoutine> Routines { get; }
	}
}
