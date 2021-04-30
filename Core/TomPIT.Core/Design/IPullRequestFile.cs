using System;

namespace TomPIT.Design
{
	public interface IPullRequestFile
	{
      int Type { get;  }
      string ContentType { get;  }
      string Content { get;  }
      string FileName { get; }
      string Topic { get; }
      string PrimaryKey { get; }
      int BlobVersion { get; }
      Guid Token { get; }
      string BaseContent { get; }
      ComponentVerb Verb { get; set; }
   }
}
