using System.Linq;
using TomPIT.Annotations.BigData;

namespace TomPIT.BigData.Data
{
	internal class PartitionSchemaDateField : PartitionSchemaField
	{
		public TimestampPrecision Precision { get; set; } = TimestampPrecision.Raw;

		public override void Initialize()
		{
			if (Attributes?.FirstOrDefault(f => f is BigDataTimestampPrecisionAttribute) is not BigDataTimestampPrecisionAttribute precision)
				return;

			Precision = precision.Precision;
		}
	}
}
