using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.BigData.Services
{
	internal class PartitionFieldStatistics : IPartitionFieldStatistics
	{
		public Guid File {get;set;}

		public string StartString {get;set;}

		public string EndString {get;set;}

		public double StartNumber {get;set;}

		public double EndNumber {get;set;}

		public DateTime StartDate {get;set;}

		public DateTime EndDate {get;set;}

		public string FieldName {get;set;}
	}
}