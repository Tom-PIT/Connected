﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.BigData.Services
{
	internal class PartitionFile : IPartitionFile
	{
		public DateTime StartTimestamp {get;set;}
		public DateTime EndTimestamp {get;set;}
		public int Count {get;set;}
		public PartitionFileStatus Status {get;set;}
		public Guid Node {get;set;}
		public Guid FileName {get;set;}
		public Guid Partition {get;set;}
		public string Key {get;set;}
	}
}