using System;

namespace TomPIT.Search
{
	internal interface IIndexingService
	{
		void Scave();
		void Ping(Guid popReceipt, int nextVisible);
		void Flush();
		void Complete(Guid popReceipt);
		ICatalogState SelectState(Guid catalog);
		void MarkRebuilding(Guid catalog);
		void ResetRebuilding(Guid catalog);
		void Rebuild(Guid catalog);
		void CompleteRebuilding(Guid catalog);
	}
}
