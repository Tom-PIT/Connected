using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TomPIT.Design
{
	public class PullRequestComponent : IPullRequestComponent
	{
		public Guid Token {get;set;}

		public string Name {get;set;}

		public string Type {get;set;}

		public Guid Folder {get;set;}

		public string Category {get;set;}

		public string Namespace {get;set;}

		public Guid RuntimeConfiguration {get;set;}

		public ComponentVerb Verb {get;set;}

		[JsonConverter(typeof(PullRequestFileConverter))]
		public List<IPullRequestFile> Files {get;set;}
	}
}
