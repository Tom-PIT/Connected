using Newtonsoft.Json;
using System;

namespace TomPIT.Deployment
{
	public class MicroServiceString
	{
		[JsonProperty(PropertyName = "element")]
		public Guid Element { get; set; }
		[JsonProperty(PropertyName = "value")]
		public string Value { get; set; }
		[JsonProperty(PropertyName = "lcid")]
		public int Lcid { get; set; }
		[JsonProperty(PropertyName = "property")]
		public string Property { get; set; }
	}
}
