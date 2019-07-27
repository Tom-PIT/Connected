using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.BigData.Services
{
	public interface IPartitionService
	{
		List<IPartition> Query();
		IPartition Select(IPartitionConfiguration configuration);
		IPartitionFile SelectFile(long id);

		void NotifyChanged(Guid token);
		void NotifyRemoved(Guid token);
	}
}
