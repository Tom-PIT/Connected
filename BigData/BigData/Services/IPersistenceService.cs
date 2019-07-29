using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.BigData.Data;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.BigData.Services
{
	internal enum MergePolicy
	{
		Partial = 1,
		Full = 2
	}
	internal interface IPersistenceService
	{
		void SynchronizeSchema(INode node, IPartitionFile file);
		PartitionSchema SelectSchema(IPartitionConfiguration configuration);

		DataTable Merge(IUpdateProvider provider, INode node, DataFileContext context, MergePolicy policy);
	}
}
