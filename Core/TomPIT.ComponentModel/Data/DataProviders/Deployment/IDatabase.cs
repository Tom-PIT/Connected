using System.Collections.Generic;

namespace TomPIT.Data.DataProviders.Deployment
{
	public interface IDatabase
	{
		List<ITable> Tables { get; }
		List<IView> Views { get; }
		List<IRoutine> Routines { get; }
	}
}
