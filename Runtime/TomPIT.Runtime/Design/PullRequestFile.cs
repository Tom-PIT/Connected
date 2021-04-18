using System;

namespace TomPIT.Design
{
	public class PullRequestFile : IPullRequestFile
	{

		public int Type {get;set;}

		public string ContentType {get;set;}

		public string Content {get;set;}

		public string FileName {get;set;}

		public string Topic {get;set;}

		public string PrimaryKey {get;set;}

		public int BlobVersion {get;set;}

		public Guid Token {get;set;}
		public string BaseContent { get; set; }
	}
}
