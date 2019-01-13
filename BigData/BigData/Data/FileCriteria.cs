using System;

namespace Amt.DataHub.Data
{
	internal class FileCriteria
	{
		public string Key { get; set; }
		public string Field { get; set; }
		public DateTime StartTimestamp { get; set; }
		public DateTime EndTimestamp { get; set; }

		public string MinString { get; set; }
		public string MaxString { get; set; }

		public double MinNumber { get; set; }
		public double MaxNumber { get; set; }

		public DateTime MinDate { get; set; }
		public DateTime MaxDate { get; set; }
	}
}
