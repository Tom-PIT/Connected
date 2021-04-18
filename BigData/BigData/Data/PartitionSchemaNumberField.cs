using System.Linq;
using TomPIT.Annotations.BigData;

namespace TomPIT.BigData.Data
{
	internal class PartitionSchemaNumberField : PartitionSchemaField
	{
		public AggregateMode Aggregate { get; set; } = AggregateMode.None;

		public override void Initialize()
		{
			if (Attributes.Count == 0)
				return;

			if (!(Attributes.FirstOrDefault(f => f is BigDataAggregateAttribute) is BigDataAggregateAttribute aggregate))
				return;

			Aggregate = aggregate.Mode;
		}

		public override int CompareTo(object obj)
		{
			if (!(obj is PartitionSchemaNumberField nf))
				return -1;

			if (Aggregate != nf.Aggregate)
				return -1;

			return base.CompareTo(obj);
		}
	}
}
