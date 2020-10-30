using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json.Linq;
using TomPIT.BigData.Data;
using TomPIT.BigData.Partitions;
using TomPIT.BigData.Transactions;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.BigData.Persistence
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

		public JArray Query(IPartitionConfiguration configuration, List<QueryParameter> parameters);
	}
}
