using System.Collections.Generic;
using TomPIT.Storage;

namespace TomPIT.SysDb.Messaging
{
	public interface IQueueHandler
	{
		List<IQueueMessage> Query();
		void Update(List<IQueueMessage> messages);
	}
}
