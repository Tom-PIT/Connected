using System.Collections.Generic;

namespace TomPIT.Data.DataProviders
{
	public enum OperationType
	{
		Other = 0,
		Query = 1,
		Select = 2,
		Insert = 3,
		Update = 4,
		Delete = 5
	}
	public interface IOrmProvider
	{
		void Synchronize(string connectionString, IModelSchema model, List<IModelOperationSchema> procedures);
		ICommandTextDescriptor Parse(string connectionString, IModelOperationSchema operation);
	}
}
