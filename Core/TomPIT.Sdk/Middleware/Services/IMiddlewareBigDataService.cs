using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareBigDataService
	{
		void Update<T>([CIP(CIP.BigDataPartitionProvider)]string partition, T item);
	}
}
