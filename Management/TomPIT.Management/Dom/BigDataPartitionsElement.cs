using System.Collections.Generic;
using System.Linq;
using TomPIT.BigData;
using TomPIT.Ide.Dom;
using TomPIT.Management.BigData;

namespace TomPIT.Management.Dom
{
	public class BigDataPartitionsElement : DomElement
	{
		public const string NodeId = "BigDataPartitions";

		private List<IPartition> _partitions = null;
		public BigDataPartitionsElement(IDomElement parent) : base(parent)
		{
			Id = NodeId;
			Title = "Partitions";

			((Behavior)Behavior).AutoExpand = false;
		}

		public override bool HasChildren => Partitions.Count > 0;
		public override int ChildrenCount => Partitions.Count;

		public override void LoadChildren()
		{
			foreach (var partition in Partitions)
				Items.Add(new BigDataPartitionElement(this, partition));
		}

		public override void LoadChildren(string id)
		{
			var partition = Partitions.FirstOrDefault(f => string.Compare(f.Configuration.ToString(), id, true) == 0);

			if (partition != null)
				Items.Add(new BigDataPartitionElement(this, partition));
		}
		private List<IPartition> Partitions
		{
			get
			{
				if (_partitions == null)
					_partitions = Environment.Context.Tenant.GetService<IBigDataManagementService>().QueryPartitions();

				return _partitions;
			}
		}
	}
}
