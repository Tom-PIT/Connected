﻿using System;

namespace TomPIT.Storage
{
	public class Blob : IBlob
	{
		public const string ContentTypeJson = "application/json";
		public Guid ResourceGroup { get; set; }
		public string FileName { get; set; }
		public Guid Token { get; set; }
		public int Size { get; set; }
		public string ContentType { get; set; }
		public string PrimaryKey { get; set; }
		public Guid MicroService { get; set; }
		public string Draft { get; set; }
		public int Version { get; set; }
		public int Type { get; set; }
		public string Topic { get; set; }
		public DateTime Modified { get; set; }
	}
}
