using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Storage
{
	internal class Blob : IBlob
	{
		public Guid ResourceGroup { get; set; }
		public string FileName { get; set; }
		public Guid Token { get; set; }
		public int Size { get; set; }
		public string ContentType { get; set; }
		public string PrimaryKey { get; set; }
		public Guid MicroService { get; set; }
		public Guid Draft { get; set; }
		public int Version { get; set; }
		public int Type { get; set; }
		public string Topic { get; set; }
		public DateTime Modified { get; set; }
	}
}
