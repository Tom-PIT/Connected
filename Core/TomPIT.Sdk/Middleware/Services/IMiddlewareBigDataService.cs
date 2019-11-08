using System.Collections.Generic;
using CAP = TomPIT.Annotations.Design.CodeAnalysisProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareBigDataService
	{
		void Add<T>([CAP(CAP.BigDataPartitionProvider)]string partition, T item);
		void Add<T>([CAP(CAP.BigDataPartitionProvider)]string partition, List<T> items);
	}
}
