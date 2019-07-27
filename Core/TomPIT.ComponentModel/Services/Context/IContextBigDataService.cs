using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Annotations;

namespace TomPIT.Services.Context
{
	public interface IContextBigDataService
	{
		void Add<T>([CodeAnalysisProvider(ExecutionContext.BigDataPartitionProvider)]string partition, T item);
		void Add<T>([CodeAnalysisProvider(ExecutionContext.BigDataPartitionProvider)]string partition, List<T> items);
	}
}
